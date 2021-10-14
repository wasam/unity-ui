using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.samwalz.unity_ui.auto_complete
{
    public class AutoCompleteWindow : MonoBehaviour, ICancelHandler
    {
        public int maxResults = 7;
        public AutoCompleteButton choicePrefab;
        
        private TMP_InputField _input;
        private RectTransform _transform;
        private IAutoCompleteSearch _search;
        
        private static AutoCompleteWindow _instance;
        private bool _initialised;
        
        public static AutoCompleteWindow Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<AutoCompleteWindow>();
                _instance.Init();
                return _instance;
            }
        }

        private void Init()
        {
            if (_initialised) return;

            _transform = transform as RectTransform;
            Hide();
            
            _instance = this;
            _initialised = true;
        }

        private void Start()
        {
            Init();
        }

        
        public void Attach(TMP_InputField input, IAutoCompleteSearch search)
        {
            if (_input == input) return;
            if (_input != null) DetachInternal(_input, input == null);
            _input = input;
            if (input == null) return;
            _search = search;
            Show(input);
            _input.onValueChanged.AddListener(OnTextChange);
            OnTextChange(_input.text);
        }

        public void Detach(TMP_InputField input = null)
        {
            DetachInternal(input);
        }

        public void ButtonChoice(string text)
        {
            if (_input == null) return;
            _input.text = text;
            Detach();
        }


        private void DetachInternal(TMP_InputField input, bool hide = true)
        {
            if (input == null) input = _input;
            if (input != _input) return;
            if (_input != null)
            {
                _input.onValueChanged.RemoveListener(OnTextChange);
                _input = null;
            }
            if (hide) Hide();
        }
        
        
        private void OnTextChange(string text)
        {
            var results = new List<string>();
            _search.Search(text, ref results, maxResults);
            UpdateChoiceButtons(results);
        }

        private void UpdateChoiceButtons(List<string> choices)
        {
            var childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var c = transform.GetChild(0);
                misc.ObjectPool.Recycle(c.gameObject);
            }
            for (int i = 0; i < choices.Count; i++)
            {
                var acb = misc.ObjectPool.InstantiateSustainable(choicePrefab, Vector3.zero, Quaternion.identity, transform);
                var go = acb.gameObject;
                acb.Text = choices[i];
                acb.autoCompleteWindow = this;
                go.transform.localScale = Vector3.one;
            }
        }
        

        private void Show(Component input)
        {
            gameObject.SetActive(true);
            var rt = input.gameObject.transform as RectTransform;
            var rect = misc.RectTransformUtility.GetWorldRect(rt);
            _transform.position = new Vector3(rect.xMin, rect.yMin, 0);
            _transform.localScale = Vector3.one;
            _transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.sizeDelta.x);
            
        }
        private void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnCancel(BaseEventData eventData)
        {
            Detach();
        }
    }
}