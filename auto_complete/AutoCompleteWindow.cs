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
        public delegate void DetachHandler(AutocompleteInputField autocompleteInputField);
        public event ChoiceHandler OnChoice;
        public event DetachHandler OnDetach;
        
        public int maxResults = 7;
        public AutoCompleteButton choicePrefab;
        
        private TMP_InputField _input;
        private RectTransform _transform;
        private ISearchProvider _searchProvider;
        private AutocompleteInputField _autocompleteInput;

        private CanvasGroup _canvasGroup;
        
        private static AutoCompleteWindow _instance;
        private bool _initialised;
        
        public static AutoCompleteWindow Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<AutoCompleteWindow>(true);
                _instance.Init();
                return _instance;
            }
        }

        private void Init()
        {
            if (_initialised) return;

            _transform = transform as RectTransform;
            _canvasGroup = GetOrAddComponent<CanvasGroup>(gameObject);
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
            var newText = choiceText;
            if (_autocompleteInput.separator != '\0')
            {
                GetCaretArea(_input, _autocompleteInput.separator, out var start, out var end, out var length);
                var text = _input.text;
                // insert new text
                newText = 
                    (start > 0 ? text.Substring(0, start) + " " : "") + 
                    choiceText + 
                    (end < text.Length ? text.Substring(end) : "");
                // cleanup a bit
                var parts = newText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                newText = string.Join(", ", parts);
                _input.text = newText;    
            }
            _input.text = newText;
            OnChoice?.Invoke(newText);
        }
        private void DetachInternal(TMP_InputField input, bool hide = true)
        {
            if (input == null) input = _input;
            if (input != _input) return;
            if (_input != null)
            {
                if (_autocompleteInput != null)
                {
                    OnDetach?.Invoke(_autocompleteInput);
                    _autocompleteInput = null;
                }
                _input.onValueChanged.RemoveListener(OnTextChange);
                _input = null;
            }
            if (hide) Hide();
        }
        
        
        private void OnTextChange(string text)
        {
            GetCaretArea(_input, _autocompleteInput.separator, out var start, out var end, out var length);
            var results = ObjectPool<List<string>>.Get();
            if (length <= 0)
            {
                HideCanvas();
                results.Clear();
            }
            else
            {
                var searchTerm = text.Substring(start, length).Trim();
                if (searchTerm.Length < _autocompleteInput.minChars)
                {
                    HideCanvas();
                }
                else
                {
                    ShowCanvas();
                    _searchProvider.Search(searchTerm, ref results, maxResults);
                }
                Debug.Log("'" + searchTerm + "'");
            }
            UpdateChoiceButtons(results);
            ObjectPool<List<string>>.Return(results);
        }

        private static void GetCaretArea(TMP_InputField input, char separator, out int start, out int end, out int length)
        {
            var text = input.text;
            if (separator == '\0' || text.Length == 0)
            {
                start = 0;
                end = text.Length - 1;
                length = text.Length;
                return;
            }
            var caretPos = input.caretPosition;
            var maxPos = text.Length;
            
            start = caretPos > 0 ? text.LastIndexOf(separator, caretPos - 1) : -1;
            var startFound = start != -1;
            end = caretPos < maxPos ? text.IndexOf(separator, caretPos) : -1;
            var endFound = end != -1;
            
            if (!endFound) end = maxPos;
            start += 1;
            
            length = end - start;
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
            ShowCanvas();
            var rt = input.gameObject.transform as RectTransform;
            var rect = misc.RectTransformUtility.GetWorldRect(rt);
            _transform.position = new Vector3(rect.xMin, rect.yMin, 0);
            _transform.localScale = Vector3.one;
            _transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.sizeDelta.x);
            
        }

        public override void Hide()
        {
            base.Hide();
            HideCanvas();
        }

        private void ShowCanvas()
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }

        private void HideCanvas()
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
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