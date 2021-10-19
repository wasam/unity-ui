using System;
using com.samwalz.unity_ui.misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.samwalz.unity_ui.searchable_dropdown
{
    public class SearchableDropdownWindow : AbstractModalWindow, ICancelHandler
    {
        private SearchableDropdownButton _button;
        private ISearchProvider _searchProvider;
        private RectTransform _transform;

        private bool _initialised;

        private void Init()
        {
            if (_initialised) return;

            _transform = transform as RectTransform;
            Detach();
            
            _initialised = true;
        }

        private void Start()
        {
            Init();
        }

        public void Attach(SearchableDropdownButton button, ISearchProvider searchProvider)
        {
            if (_button == button) return;
            if (_button != null) DetachInternal(_button, button == null);
            _button = button;
            if (button == null) return;
            _searchProvider = searchProvider;
            Show(button);
        }

        public void Detach(SearchableDropdownButton button = null)
        {
            DetachInternal(button);
        }

        public void ButtonChoice(string text)
        {
            if (_button == null) return;
            Detach();
        }


        private void DetachInternal(SearchableDropdownButton button, bool hide = true)
        {
            if (button == null) button = _button;
            if (button != _button) return;
            if (_button != null)
            {
                _button = null;
            }
            if (hide) Hide();
        }
        
        private void Show(Component input)
        {
            base.Show();
            gameObject.SetActive(true);
            var rt = input.gameObject.transform as RectTransform;
            var rect = misc.RectTransformUtility.GetWorldRect(rt);
            _transform.position = new Vector3(rect.xMin, rect.yMax, 0);
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