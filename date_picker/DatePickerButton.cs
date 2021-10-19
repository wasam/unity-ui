using System;
using System.Globalization;
using com.samwalz.unity_ui.misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.samwalz.unity_ui.date_picker
{
    [RequireComponent(typeof(Button))]
    public class DatePickerButton : MonoBehaviour
    {
        private Button _button;
        private TextMeshProUGUI _buttonLabel;
        
        private bool _initialised;

        private DateTime _dateTime;
        private bool _dateTimeSet;
        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                _dateTime = value;
                UpdateButton();
            }
        }

        public bool DateTimeSet
        {
            get => _dateTimeSet;
            set
            {
                _dateTimeSet = value;
                UpdateButton();
            }
            
        }

        private void UpdateButton()
        {
            _buttonLabel.text = _dateTimeSet ? _dateTime.ToString("y", DateTimeFormatInfo.InvariantInfo) : "- not set -";
        }

        private void Init()
        {
            if (_initialised) return;

            _button = GetComponent<Button>();
            _buttonLabel = GetComponentInChildren<TextMeshProUGUI>();
            _button.onClick.AddListener(OnClick);
            
            _initialised = true;
        }
        
        // Start is called before the first frame update
        private void Start()
        {
            Init();
        }

        private void OnClick()
        {
            DatePickerWindow.Instance.Attach(this);
        }

        

    }
}
