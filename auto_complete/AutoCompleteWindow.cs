using System;
using System.Collections.Generic;
using com.samwalz.unity_ui.misc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.samwalz.unity_ui.auto_complete
{
    public class AutoCompleteWindow : AbstractModalWindow, ICancelHandler
    {
        public delegate void ChoiceHandler(string choice);
        public event ChoiceHandler OnChoice;
        
        public int maxResults = 7;
        public AutoCompleteButton choicePrefab;
        
        private TMP_InputField _input;
        private RectTransform _transform;
        private ISearchProvider _searchProvider;
        private AutocompleteInputField _autocompleteInput;
        
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

        
        public void Attach(
            AutocompleteInputField autocompleteInputField, 
            TMP_InputField input, 
            ISearchProvider searchProvider
            )
        {
            if (_input == input) return;
            if (_input != null) DetachInternal(_input, input == null);
            _input = input;
            if (input == null) return;
            _autocompleteInput = autocompleteInputField;
            _searchProvider = searchProvider;
            Show(input);
            _input.onValueChanged.AddListener(OnTextChange);
            OnTextChange(_input.text);
        }

        public void Detach(TMP_InputField input = null)
        {
            DetachInternal(input);
        }
        public void ButtonChoice(string choiceText)
        {
            GetCaretArea(_input, _autocompleteInput.separator, out var start, out var end, out var length);
            var text = _input.text;
            Debug.Log(start + " " + end + " " + length);
            var newText = text.Substring(0, start + 1) + " " + choiceText + (end > start ? text.Substring(end) : "");
            _input.text = newText;
            OnChoice?.Invoke(text);
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
            GetCaretArea(_input, _autocompleteInput.separator, out var start, out var end, out var length);
            if (length < 0) return;
            var segment = text.Substring(start, length).Trim();
            var results = new List<string>();
            _searchProvider.Search(segment, ref results, maxResults);
            UpdateChoiceButtons(results);
        }

        private static void GetCaretArea(TMP_InputField input, char separator, out int start, out int end, out int length)
        {
            var text = input.text;
            var caretPos = input.caretPosition;
            var maxPos = text.Length;
            start = text.LastIndexOf(separator, caretPos - 1);
            end =  caretPos < maxPos ? text.IndexOf(separator, caretPos - 1) : -1;
            if (start == -1) start = 0;
            if (end == -1) end = text.Length - 1;
            else end -= 1;
            length = end - start;
            if (start > 0 && length > 0) start += 1;
        }

        private void UpdateChoiceButtons(List<string> choices)
        {
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var c = transform.GetChild(0);
                misc.ObjectPool.Recycle(c.gameObject);
            }
            for (var i = 0; i < choices.Count; i++)
            {
                var acb = ObjectPool.InstantiateSustainable(choicePrefab, Vector3.zero, Quaternion.identity, transform);
                var go = acb.gameObject;
                acb.Text = choices[i];
                acb.autoCompleteWindow = this;
                go.transform.localScale = Vector3.one;
            }
        }
        

        private void Show(Component input)
        {
            base.Show();
            gameObject.SetActive(true);
            var rt = input.gameObject.transform as RectTransform;
            var rect = misc.RectTransformUtility.GetWorldRect(rt);
            _transform.position = new Vector3(rect.xMin, rect.yMin, 0);
            _transform.localScale = Vector3.one;
            _transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.sizeDelta.x);
            
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
            Detach();
        }
    }
}