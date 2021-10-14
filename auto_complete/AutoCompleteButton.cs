using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.samwalz.unity_ui.auto_complete
{
    [RequireComponent(typeof(Button))]
    public class AutoCompleteButton : MonoBehaviour
    {
        public TextMeshProUGUI buttonText;
        public AutoCompleteWindow autoCompleteWindow;
        private string _text;
        
        public string Text
        {
            set
            {
                _text = value;
                buttonText.text = value;
            }
        }
        
        public void OnClick()
        {
            autoCompleteWindow.ButtonChoice(_text);
        }
        
    }
}