using System;
using System.Globalization;
using com.samwalz.unity_ui.misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.samwalz.unity_ui.date_picker
{
    [RequireComponent(typeof(Button))]
    public class DatePickerButton : AbstractModalWindow
    {
        private Button _button;
        private TextMeshProUGUI _buttonLabel;
        
        private bool _initialised;

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                _dateTime = value;
                _buttonLabel.text = _dateTime.ToString("y", DateTimeFormatInfo.InvariantInfo);
            }
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
            Show(DatePickerWindow.Instance.gameObject);
        }

        public override void Hide()
        {
            base.Hide();
            DatePickerWindow.Instance.Detach();
        }
    }
}
