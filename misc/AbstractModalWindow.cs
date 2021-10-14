using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.samwalz.unity_ui.misc
{
    public abstract class AbstractModalWindow : MonoBehaviour
    {
        private GameObject _windowGameObject;
        private GameObject _clickBlocker;

        private Canvas _rootCanvas;

        public virtual void Show(GameObject windowGameObject)
        {
            _windowGameObject = windowGameObject;
            if (_windowGameObject == null) return;
            // GetOrAddComponent<Canvas>(_windowGameObject);
            var popupCanvas = GetOrAddComponent<Canvas>(_windowGameObject);
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 20000;
            TakeCareOfRaycaster(_windowGameObject, GetParentCanvas(_windowGameObject.transform.parent));
            if (_rootCanvas == null) _rootCanvas = GetRootCanvas(gameObject);
            _clickBlocker = CreateClickBlock(_rootCanvas);
        }

        public virtual void Hide()
        {
            if (_clickBlocker != null)
            {
                Destroy(_clickBlocker);
                _clickBlocker = null;
            }
        }

        
        private GameObject CreateClickBlock(Canvas rootCanvas)
        {
            var blocker = new GameObject("ClickBlock");

            // Setup blocker RectTransform to cover entire root canvas area.
            var blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            var blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            var dropdownCanvas = _windowGameObject.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            
            TakeCareOfRaycaster(blocker, GetParentCanvas(transform.parent));


            // Add image since it's needed to block, but make it clear.
            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            var blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        private static void TakeCareOfRaycaster(GameObject go, Canvas parentCanvas)
        {
            if (parentCanvas != null)
            {
                var components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    var raycasterType = components[i].GetType();
                    GetOrAddComponent(go, raycasterType);
                }
            }
            else
            {
                GetOrAddComponent<GraphicRaycaster>(go);
            }
        }
        
        private static T GetOrAddComponent<T>(GameObject go) where T: Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        private static Component GetOrAddComponent(GameObject go, Type t)
        {
            var c = go.GetComponent(t);
            if (c == null) c = go.AddComponent(t);
            return c;
        }

        private static Canvas GetParentCanvas(Transform parentTransform)
        {
            Canvas parentCanvas = null;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;
                parentTransform = parentTransform.parent;
            }
            return parentCanvas;
        }
        private static Canvas GetRootCanvas(GameObject go)
        {
            var candidates = ObjectPool<List<Canvas>>.Get();
            go.GetComponentsInParent(true, candidates);
            var rootCanvas = candidates.FirstOrDefault(t => t.isRootCanvas);
            ObjectPool<List<Canvas>>.Return(candidates);
            return rootCanvas;
        }
    }
}