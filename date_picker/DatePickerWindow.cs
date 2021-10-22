using System;
using System.Collections;
using System.Collections.Generic;
using com.samwalz.unity_ui.misc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.samwalz.unity_ui.date_picker
{
    public class DatePickerWindow : AbstractModalWindow, ICancelHandler
    {
        public TMP_InputField inputYear;
        public TMP_Dropdown ddMonth;
        public TMP_Dropdown ddDay;

        
        
        private RectTransform _transform;
        private DatePickerButton _input;

        private bool _ignoreChanges;

        private DateTime _dateTime;
        
        private bool _initialised;
        private static DatePickerWindow _instance;
    
        public static DatePickerWindow Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<DatePickerWindow>(true);
                _instance.Init();
                return _instance;
            }
        }
        private int Year
        {
            get => _dateTime.Year;
            set =>
                _dateTime = new DateTime(
                    value,
                    _dateTime.Month,
                    _dateTime.Day
                );
        }
        private int Month
        {
            get => _dateTime.Month;
            set =>
                _dateTime = new DateTime(
                    _dateTime.Year,
                    value,
                    _dateTime.Day
                );
        }

        private void OnYearEndEdit(string value)
        {
            if (_ignoreChanges) return;
            if (int.TryParse(value, out var year))
            {
                if (Year == year) return;
                Year = year;
            }
            UpdateInputs();
        }

        private void OnMonthEndEdit(int value)
        {
            if (_ignoreChanges) return;
            var month = value + 1;
            if (Month == month) return;
            Month = month;
            UpdateInputs();
        }

        private void Init()
        {
            if (_initialised) return;
    
            _transform = transform as RectTransform;
            inputYear.onEndEdit.AddListener(OnYearEndEdit);
            ddMonth.onValueChanged.AddListener(OnMonthEndEdit);
            Hide();
    
            _instance = this;
            _initialised = true;
        }
        
        // Start is called before the first frame update
        private void Start()
        {
            Init();
        }

        public void Apply()
        {
            if (_input == null) return;
            _input.DateTime = _dateTime;
            _input.DateTimeSet = true;
            Close();
        }
        public void Cancel()
        {
            Close();
        }
        public void RemoveDate()
        {
            _input.DateTimeSet = false;
            Close();
        }
        
        
        
        public void Attach(DatePickerButton input)
        {
            if (_input == input) return;
            if (_input != null) DetachInternal(_input, input == null);
            _input = input;
            if (input == null) return;
            _dateTime = input.DateTimeSet ? input.DateTime : DateTime.Now;
            UpdateInputs();
            Show(input);
        }
    
        public void Detach(DatePickerButton input = null)
        {
            DetachInternal(input);
        }

        private void UpdateInputs()
        {
            _ignoreChanges = true;
            inputYear.text = _dateTime.Year.ToString("0000");
            ddMonth.value = _dateTime.Month - 1;
            _ignoreChanges = false;
        }
    
        private void DetachInternal(DatePickerButton input, bool hide = true)
        {
            if (input == null) input = _input;
            if (input != _input) return;
            if (_input != null)
            {
                _input = null;
            }
            if (hide) Hide();
        }

        private void Close()
        {
            Detach();
        }
    
        private void Show(Component input)
        {
            base.Show();
            gameObject.SetActive(true);
            var rt = input.gameObject.transform as RectTransform;
            var rect = misc.RectTransformUtility.GetWorldRect(rt);
            _transform.position = new Vector3(rect.xMin, rect.yMin, 0);
            _transform.localScale = Vector3.one;
            // _transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.sizeDelta.x);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.SetActive(false);
        }

        protected override void OnBlockerClick()
        {
            Detach();
        }

        public void OnCancel(BaseEventData eventData)
        {
            Cancel();
        }
    }
}

