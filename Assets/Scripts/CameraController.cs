using PolePosition.Player;
using UnityEngine;

namespace PolePosition
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] public GameObject m_Focus;

        [SerializeField] public Vector3 m_offset = new Vector3(10, 10, 10);

        [SerializeField] public CircuitController m_Circuit;
        [SerializeField] private float m_Distance = 10;
        [SerializeField] private float m_Elevation = 8;
        [Range(0, 1)] [SerializeField] private float m_Following = 0.5f;

        private Vector3 m_Direction = Vector3.zero;

        private Camera mainCamera;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = this.GetComponent<Camera>();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            CalculateCameraPosition();
        }

        private void CalculateCameraPosition()
        {
            if (m_Focus != null)
            {
                if (this.m_Circuit != null)
                {
                    if (this.m_Direction.magnitude == 0)
                    {
                        this.m_Direction = new Vector3(0f, -1f, 0f);
                    }

                    int segIdx;
                    float carDist;
                    Vector3 carProj;
                    Vector3 carDirection;

                    m_Circuit.ComputeClosestPointArcLength(m_Focus.transform.position,out carDirection, out segIdx, out carProj,
                        out carDist);

                    Vector3 pathDir = -m_Circuit.GetSegment(segIdx);
                    pathDir = new Vector3(pathDir.x, 0f, pathDir.z);
                    pathDir.Normalize();

                    this.m_Direction = Vector3.Lerp(this.m_Direction, pathDir, this.m_Following * Time.deltaTime);
                    Vector3 offset = this.m_Direction * this.m_Distance;
                    offset = new Vector3(offset.x, m_Elevation, offset.z);

                    transform.position = m_Focus.transform.position + offset;
                    transform.LookAt(m_Focus.transform.position);
                }
                else
                {
                    transform.position = m_Focus.transform.position + m_offset;
                    transform.LookAt(m_Focus.transform.position);
                }
            }
        }

        public void Reset()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }
}