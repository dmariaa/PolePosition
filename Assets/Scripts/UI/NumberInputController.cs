using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PolePosition.UI
{
    class NumberInputController : MonoBehaviour
    {
        /// <summary>
        /// Relevant UI elements
        /// </summary>
        private Text _valueText;
        private Button _buttonPlus;
        private Button _buttonMinus;

        public int Value
        {
            get => int.Parse(_valueText.text);
            set
            {
                if (OnUpdateNumberValidate != null)
                {
                    if (OnUpdateNumberValidate(value))
                    {
                        _valueText.text = "" + value;

                    }
                }
                else
                {
                    _valueText.text = "" + value;
                }
            }
        }

        /// <summary>
        /// Delegate called when numDrivers is updated
        /// </summary> 
        public delegate void UpdateNumberInputDelegate(int value);
        public UpdateNumberInputDelegate OnUpdateNumberInput;

        /// <summary>
        /// 
        /// </summary> 
        public delegate bool UpdateNumberValidateDelegate(int value);
        public UpdateNumberValidateDelegate OnUpdateNumberValidate;

        private void Awake()
        {
            _valueText = transform.Find("ValueText").GetComponent<Text>();
            _buttonMinus = transform.Find("ButtonMinus").GetComponent<Button>();
            _buttonPlus = transform.Find("ButtonPlus").GetComponent<Button>();

            _buttonMinus.onClick.AddListener(() =>
            {
                Value--;
                if (OnUpdateNumberInput != null)
                {
                    OnUpdateNumberInput(Value);
                }
            });

            _buttonPlus.onClick.AddListener(() =>
            {
                Value++;
                if (OnUpdateNumberInput != null)
                {
                    OnUpdateNumberInput(Value);
                }
            });
        }

        /// <summary>
        /// Enables or disables Configuration Button
        /// </summary>
        /// <param name="enable">enables if true, disables if false</param>
        public void EnableNumberInputButtons(bool enable)
        {
            _buttonMinus.gameObject.SetActive(enable);
            _buttonPlus.gameObject.SetActive(enable);
        }
    }
}
