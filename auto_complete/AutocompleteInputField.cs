using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.samwalz.unity_ui.auto_complete
{
    [RequireComponent(typeof(TMP_InputField))]
    public class AutocompleteInputField : MonoBehaviour, IPointerClickHandler
    {
        public bool showAutoComplete = true;
        public int minChars = 3; 
        
        public IAutoCompleteSearch Search;
        
        private AutoCompleteWindow _autoCompleteWindow;
        private bool _autoCompleteWindowAttached;
        private TMP_InputField _input;
        private bool _initialised;

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

        public void OnPointerClick(PointerEventData eventData)
        {
            _autoCompleteWindow.Detach();
        }

        private void CheckAttachDetach()
        {
            if (!_input.isFocused) return;
            if (_input.text.Trim().Length >= minChars &&
                showAutoComplete)
            {
                _autoCompleteWindow.Attach(_input, Search);
                _autoCompleteWindowAttached = true;
            }
            else
            {
                _autoCompleteWindow.Detach();
            }
            
        }
    }
}