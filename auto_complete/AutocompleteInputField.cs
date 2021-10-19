using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.samwalz.unity_ui.auto_complete
{
    [RequireComponent(typeof(TMP_InputField))]
    public class AutocompleteInputField : MonoBehaviour
    {
        public event AutoCompleteWindow.ChoiceHandler OnChoice;
        
        public bool showAutoComplete = true;
        public int minChars = 3;
        public char separator = ',';
        
        public misc.ISearchProvider SearchProvider;
        
        private AutoCompleteWindow _autoCompleteWindow;
        private bool _autoCompleteWindowAttached;
        private TMP_InputField _input;
        private bool _initialised;

        public string Text
        {
            get => _input.text;
            set => _input.text = value;
        }
        
        private void Init()
        {
            if (_initialised) return;

            _autoCompleteWindow = AutoCompleteWindow.Instance;
            _input = GetComponent<TMP_InputField>();
            _input.onValueChanged.AddListener(OnTextChange);
            _initialised = true;
        }
        private void Start()
        {
            Init();
        }
        
        private void OnTextChange(string text)
        {
            CheckAttachDetach();
        }
        private void CheckAttachDetach()
        {
            if (!_input.isFocused) return;
            if (_input.text.Trim().Length >= minChars &&
                showAutoComplete)
            {
                Attach();
            }
            else
            {
                Detach();
            }
            
        }

        private void Attach()
        {
            _autoCompleteWindow.Attach(this, _input, SearchProvider);
            _autoCompleteWindow.OnChoice += AutoCompleteWindowOnOnChoice;
            _autoCompleteWindowAttached = true;
        }

        private void AutoCompleteWindowOnOnChoice(string choice)
        {
            _autoCompleteWindow.Detach();
            OnChoice?.Invoke(choice);
        }

        private void Detach()
        {
            _autoCompleteWindow.Detach();
            _autoCompleteWindow.OnChoice -= AutoCompleteWindowOnOnChoice;
            _autoCompleteWindowAttached = false;
        }
    }
}