using System;
using System.Collections.Generic;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.UIToolkitElements
{
    public abstract class ScenePickerBaseElement<T>: BindableElement, INotifyValueChanged<T>
    {
        private static VisualTreeAsset _sceneElement;
        // public readonly Button Button;
        protected readonly ObjectField _sceneField;

        private VisualElement _dropdownRoot;

        public VisualElement DropdownRoot
        {
            get => _dropdownRoot ?? this;
            set => _dropdownRoot = value;
        }

        public ScenePickerBaseElement()
        {
            if (_sceneElement == null)
            {
                _sceneElement = Util.LoadResource<VisualTreeAsset>("UIToolkit/AddressableScene.uxml");
            }

            TemplateContainer dropdownElement = _sceneElement.CloneTree();
            dropdownElement.style.flexGrow = 1;

            Button button = dropdownElement.Q<Button>();
            button.clicked += MakeDropdown;
            _sceneField = dropdownElement.Q<ObjectField>();

            _sceneField.RegisterValueChangedCallback(OnSceneFieldChanged);

            Add(dropdownElement);
        }

        protected abstract bool AllowEmpty();
        protected abstract IScenePickerPayload MakeEmpty();
        protected abstract bool CurrentIsPayload(IScenePickerPayload payload);
        protected abstract void PostProcessDropdownList(AdvancedDropdownList<IScenePickerPayload> dropdown);

        protected const string EditorIconPath = "d_editicon.sml";

        private void MakeDropdown()
        {
            AdvancedDropdownList<IScenePickerPayload> dropdown = new AdvancedDropdownList<IScenePickerPayload>();
            if (AllowEmpty())
            {
                dropdown.Add("[Empty]", MakeEmpty());
                dropdown.AddSeparator();
            }

            bool selected = false;
            IScenePickerPayload selectedResult = default;
            foreach (IScenePickerPayload payload in GetScenePickerPayloads())
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(new AdvancedDropdownList<IScenePickerPayload>(payload.Name, payload));

                if (CurrentIsPayload(payload))
                {
                    selected = true;
                    selectedResult = payload;
                }
            }

            PostProcessDropdownList(dropdown);
            // dropdown.AddSeparator();
            // dropdown.Add("Edit Scenes In Build...", ("", -2), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selected ? new object[] { selectedResult } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect wb, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(DropdownRoot.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                DropdownRoot.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    SetSelectedPayload((IScenePickerPayload) curItem);
                    // (string path, int index) = ((string path, int index))curItem;
                    // switch (index)
                    // {
                    //     case -1:
                    //     {
                    //         Debug.Assert(isString);
                    //         property.stringValue = "";
                    //         ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, "");
                    //         property.serializedObject.ApplyModifiedProperties();
                    //         onValueChangedCallback.Invoke("");
                    //     }
                    //         break;
                    //     case -2:
                    //     {
                    //         SceneUtils.OpenBuildSettings();
                    //     }
                    //         break;
                    //     default:
                    //     {
                    //         if (isString)
                    //         {
                    //             property.stringValue = path;
                    //             ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, path);
                    //             property.serializedObject.ApplyModifiedProperties();
                    //             onValueChangedCallback.Invoke(path);
                    //         }
                    //         else
                    //         {
                    //             property.intValue = index;
                    //             ReflectUtils.SetValue(property.propertyPath, property.serializedObject.targetObject, info, parent, index);
                    //             property.serializedObject.ApplyModifiedProperties();
                    //             onValueChangedCallback.Invoke(index);
                    //         }
                    //     }
                    //         break;
                    // }

                    return null;
                }
            );

            UnityEditor.PopupWindow.Show(wb, sa);
        }

        protected T CachedValue;
        protected bool HasCachedValue;

        public abstract void SetValueWithoutNotify(T newValue);

        public T value
        {
            get => HasCachedValue? CachedValue: default;
            set
            {
                if (HasCachedValue && EqualityComparer<T>.Default.Equals(value, CachedValue))
                {
                    return;
                }

                T previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<T> evt = ChangeEvent<T>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        protected abstract IReadOnlyList<IScenePickerPayload> GetScenePickerPayloads();
        protected abstract void SetSelectedPayload(IScenePickerPayload scenePickerPayload);

        protected abstract void OnSceneFieldChanged(ChangeEvent<Object> evt);
    }
}
