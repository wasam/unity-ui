using System;
using com.samwalz.unity_ui.misc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.samwalz.unity_ui.searchable_dropdown
{
    [RequireComponent(typeof(Button))]
    public class SearchableDropdownButton : MonoBehaviour
    {
        public SearchableDropdownWindow dropdownWindow;
        public TextMeshProUGUI buttonLabel;
        
        private Button _button;
        private bool _initialised;

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value.Trim();
                TextSet = string.IsNullOrWhiteSpace(_text);
                buttonLabel.text = TextSet ? _text : "- not set -";
            }
        }

        public bool TextSet { get; private set; }

        private void Init()
        {
            if (_initialised) return;

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
            
            _initialised = true;
        }

        private void Start()
        {
            Init();
        }

        private void OnButtonClick()
        {
            dropdownWindow.Attach(this, null);
        }
    }
}