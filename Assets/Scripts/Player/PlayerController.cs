using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

namespace PolePosition.Player
{
    public class PlayerController : NetworkBehaviour
    {
        #region Variables

        [Header("Movement")] public List<AxleInfo> axleInfos;
        public float forwardMotorTorque = 100000;
        public float backwardMotorTorque = 50000;
        public float maxSteeringAngle = 15;
        public float engineBrake = 1e+12f;
        public float footBrake = 1e+24f;
        public float topSpeed = 200f;
        public float downForce = 1000f;
        public float slipLimit = 0.2f;
        public float inputEndZone = float.Epsilon;

        // Entrada del usuario 
        [field: SerializeField] public float CurrentRotation { get; set; }
        [field: SerializeField] public float InputAcceleration { get; set; }
        [field: SerializeField] public float InputSteering { get; set; }
        [field: SerializeField] public float InputBrake { get; set; }
        
        /// <summary>
        /// Is the engine started?
        /// </summary>
        public bool EngineStarted { get; set; }
        
        /// <summary>
        /// Is the car upside down?
        /// True if car is upside down
        /// False if not
        /// Readonly
        /// </summary>
        private bool UpsideDown
        {
            get
            {
                var rotation = transform.rotation;
                return Mathf.Abs(rotation.eulerAngles.z) >= 50f &&
                       Mathf.Abs(rotation.eulerAngles.z) <= 310f;
            }
        }

        /// <summary>
        /// Is the car stopped?
        /// True if car is stopped
        /// False if not
        /// </summary>
        private bool Stopped
        {
            get
            {
                return m_Rigidbody.velocity.magnitude <= 0.01f;
            }    
        }
        
        // Componentes
        private PlayerInfo m_PlayerInfo;
        private Rigidbody m_Rigidbody;

        private float m_SteerHelper = 0.8f;
        private float _currentLapTime = 0f;
        private bool _countingTime;
        #endregion Variables

        #region Unity Callbacks

        public void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_PlayerInfo = GetComponent<PlayerInfo>();
            EngineStarted = true;
        }
        
        /// <summary>
        /// Starts counting lap time
        /// Server only
        /// </summary>
        [Server]
        public void StartLapTime()
        {
            _countingTime = true;
            _currentLapTime = Time.deltaTime;
        }

        /// <summary>
        /// Stops counting lap time
        /// Server only
        /// </summary>
        [Server]
        public void StopLapTime()
        {
            _countingTime = false;
        }
        
        [ServerCallback]
        public void Update()
        {
            if (_countingTime)
            {
                _currentLapTime += Time.deltaTime;
                m_PlayerInfo.CurrentLapTime = _currentLapTime;
                m_PlayerInfo.TotalRaceTime += Time.deltaTime;
            }
        }
        
        [ServerCallback]
        public void FixedUpdate()
        {
            float steering = maxSteeringAngle * InputSteering;

            if (!EngineStarted)
            {
                InputAcceleration = 0f;
                InputSteering = 0f;
                InputBrake = 999999999f;
            }

            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }

                if (axleInfo.motor)
                {
                    if (InputAcceleration > inputEndZone)
                    {
                        axleInfo.leftWheel.motorTorque = forwardMotorTorque;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.motorTorque = forwardMotorTorque;
                        axleInfo.rightWheel.brakeTorque = 0;
                    } else if (InputAcceleration < -inputEndZone)
                    {
                        axleInfo.leftWheel.motorTorque = -backwardMotorTorque;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.motorTorque = -backwardMotorTorque;
                        axleInfo.rightWheel.brakeTorque = 0;
                    } else
                    {
                        axleInfo.leftWheel.motorTorque = 0;
                        axleInfo.leftWheel.brakeTorque = engineBrake;
                        axleInfo.rightWheel.motorTorque = 0;
                        axleInfo.rightWheel.brakeTorque = engineBrake;
                    }

                    if (InputBrake > 0)
                    {
                        axleInfo.leftWheel.brakeTorque = footBrake;
                        axleInfo.rightWheel.brakeTorque = footBrake;

                        if(UpsideDown && Stopped)      
                        {
                            RelocateCar();
                        }
                    }
                }

                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }

            SteerHelper();
            SpeedLimiter();
            AddDownForce();
            TractionControl();

            m_PlayerInfo.Speed = m_Rigidbody.velocity;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Disable this rigidBody so no collisions and
        /// other physics act on it
        /// </summary>
        public void disableRigidBody()
        {
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.detectCollisions = false;
        }

        /// <summary>
        /// Enables this rigidBody so no collisions and other physics
        /// act on it
        /// </summary>
        public void enableRigidBody()
        {
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.detectCollisions = true;
        }

        /// <summary>
        /// Relocates car in track
        /// Server only 
        /// </summary>
        [Server]
        private void RelocateCar()
        {
            Vector3 pos = new Vector3(m_PlayerInfo.CurrentCircuitPosition.x, 0.51f, m_PlayerInfo.CurrentCircuitPosition.z);
            Vector3 rot = m_PlayerInfo.LookAtPoint;
            disableRigidBody();
            transform.position = pos;
            transform.LookAt(new Vector3(rot.x, transform.position.y, rot.z));
            enableRigidBody();
        }

        /// <summary>
        /// crude traction control that reduces the power to wheel if the car is wheel spinning too much
        /// Server only 
        /// </summary>
        [Server]
        private void TractionControl()
        {
            foreach (var axleInfo in axleInfos)
            {
                WheelHit wheelHitLeft;
                WheelHit wheelHitRight;
                axleInfo.leftWheel.GetGroundHit(out wheelHitLeft);
                axleInfo.rightWheel.GetGroundHit(out wheelHitRight);

                if (wheelHitLeft.forwardSlip >= slipLimit)
                {
                    var howMuchSlip = (wheelHitLeft.forwardSlip - slipLimit) / (1 - slipLimit);
                    axleInfo.leftWheel.motorTorque -= axleInfo.leftWheel.motorTorque * howMuchSlip * slipLimit;
                }

                if (wheelHitRight.forwardSlip >= slipLimit)
                {
                    var howMuchSlip = (wheelHitRight.forwardSlip - slipLimit) / (1 - slipLimit);
                    axleInfo.rightWheel.motorTorque -= axleInfo.rightWheel.motorTorque * howMuchSlip * slipLimit;
                }
            }
        }

        /// <summary>
        /// crude traction control that reduces the power to wheel if the car is wheel spinning too much
        /// Server only
        /// </summary>
        [Server]
        private void AddDownForce()
        {
            // foreach (var axleInfo in axleInfos)
            // {
            //     axleInfo.leftWheel.attachedRigidbody.AddForce(-transform.up * (downForce * axleInfo.leftWheel.attachedRigidbody.velocity.magnitude));
            // }
            
            // More efficient code, same calculation
            Vector3 force = -transform.up * (axleInfos.Count * (downForce * m_Rigidbody.velocity.magnitude));
            m_Rigidbody.AddForce(force);
        }

        /// <summary>
        /// Limits speed to top speed
        /// Server only
        /// </summary>
        [Server]
        private void SpeedLimiter()
        {
            float speed = m_Rigidbody.velocity.magnitude;
            if (speed > topSpeed)
                m_Rigidbody.velocity = topSpeed * m_Rigidbody.velocity.normalized;
        }

        /// <summary>
        /// finds the corresponding visual wheel
        /// correctly applies the transform
        /// </summary>
        [Server]
        public void ApplyLocalPositionToVisuals(WheelCollider col)
        {
            if (col.transform.childCount == 0)
            {
                return;
            }

            Transform visualWheel = col.transform.GetChild(0);
            Vector3 position;
            Quaternion rotation;
            col.GetWorldPose(out position, out rotation);
            var myTransform = visualWheel.transform;
            myTransform.position = position;
            myTransform.rotation = rotation;
        }

        /// <summary>
        /// Steer helper function
        /// Server only
        /// </summary>
        [Server]
        private void SteerHelper()
        {
            foreach (var axleInfo in axleInfos)
            {
                WheelHit[] wheelHit = new WheelHit[2];
                axleInfo.leftWheel.GetGroundHit(out wheelHit[0]);
                axleInfo.rightWheel.GetGroundHit(out wheelHit[1]);
                foreach (var wh in wheelHit)
                {
                    if (wh.normal == Vector3.zero)
                        return; // wheels arent on the ground so dont realign the rigidbody velocity
                }
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(CurrentRotation - transform.eulerAngles.y) < 10f)
            {
                var turnAdjust = (transform.eulerAngles.y - CurrentRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
                m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
            }

            CurrentRotation = transform.eulerAngles.y;
        }
        #endregion

        // Debugging collissions
        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.name.StartsWith("Player"))
            {
                Debug.LogFormat("Collision between {0} and {1}, generated in {2}", name, collision.gameObject.name,
                    isServer ? "Server" : "Client");
            }
        }
    }
}