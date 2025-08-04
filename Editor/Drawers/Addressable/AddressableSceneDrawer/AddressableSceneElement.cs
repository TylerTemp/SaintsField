#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Addressable.AddressableSceneDrawer
{
    public class AddressableSceneElement: BindableElement, INotifyValueChanged<string>
    {
        private readonly AddressableSceneAttribute _addressableSceneAttribute;
        private static VisualTreeAsset _addressableSceneElement;
        private readonly ObjectField _sceneField;

        public readonly UnityEvent<string> ErrorEvent = new UnityEvent<string>();
        public readonly UnityEvent<string> SceneFieldDropChanged = new UnityEvent<string>();
        public string Error = "";
        public readonly Button Button;

        public AddressableSceneElement(AddressableSceneAttribute addressableSceneAttribute)
        {
            _addressableSceneAttribute = addressableSceneAttribute;

            if (_addressableSceneElement == null)
            {
                _addressableSceneElement = Util.LoadResource<VisualTreeAsset>("UIToolkit/AddressableScene.uxml");
            }

            TemplateContainer dropdownElement = _addressableSceneElement.CloneTree();
            dropdownElement.style.flexGrow = 1;

            Button = dropdownElement.Q<Button>();
            _sceneField = dropdownElement.Q<ObjectField>();

            _sceneField.RegisterValueChangedCallback(OnSceneFieldChanged);

            Add(dropdownElement);
        }

        private void OnSceneFieldChanged(ChangeEvent<Object> evt)
        {
            SceneAsset sceneAsset = (SceneAsset)evt.newValue;
            (string error, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(_addressableSceneAttribute.Group, _addressableSceneAttribute.LabelFilters);
            if (error != "")
            {
                ErrorEvent.Invoke(Error = error);
                return;
            }

            foreach (AddressableAssetEntry addressableAssetEntry in assetGroups)
            {
                if (ReferenceEquals(addressableAssetEntry.MainAsset, sceneAsset))
                {
                    // value = addressableAssetEntry.address;
                    SceneFieldDropChanged.Invoke(addressableAssetEntry.address);
                    return;
                }
            }

            ErrorEvent.Invoke(Error = $"{sceneAsset} is not an addressable scene");
        }

        private string _cachedValue;

        public void SetValueWithoutNotify(string newValue)
        {
            _cachedValue = newValue;

            if (string.IsNullOrEmpty(newValue))
            {
                ErrorEvent.Invoke(Error = "");
                return;
            }

            (string _, IEnumerable<AddressableAssetEntry> assetGroups) = AddressableUtil.GetAllEntries(_addressableSceneAttribute.Group, _addressableSceneAttribute.LabelFilters);

            foreach (AddressableAssetEntry assetEntry in assetGroups)
            {
                if (assetEntry.address == newValue)
                {
                    Debug.Log($"{assetEntry.MainAsset}");
                    _sceneField.SetValueWithoutNotify(assetEntry.MainAsset);
                    ErrorEvent.Invoke(Error = "");
                    return;
                }
            }

            ErrorEvent.Invoke(Error = $"Not an addressable scene: {newValue}");
        }

        public string value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                string previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<string> evt = ChangeEvent<string>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }

    public class AddressableSceneField: BaseField<string>
    {
        // public readonly Button Button;

        public AddressableSceneField(string label, AddressableSceneElement addressableSceneElement) : base(label, addressableSceneElement)
        {
            // Button = addressableSceneElement.Button;
            AddToClassList(alignedFieldUssClassName);
            AddToClassList(SaintsPropertyDrawer.ClassAllowDisable);
        }
    }
}
#endif
