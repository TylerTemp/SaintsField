#if UNITY_2021_3_OR_NEWER
using System;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ListDrawerSettings
{
    public partial class ListDrawerSettingsRenderer
    {
        private SerializedListElement _element;
        private (int min, int max) _sizeLimits = (-1, -1);

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            SerializedProperty property = FieldWithInfo.SerializedProperty;
            Type elementType = ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ?? FieldWithInfo.PropertyInfo.PropertyType);
            PropertyAttribute[] attributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(FieldWithInfo.FieldInfo);
            ListDrawerSettingsAttribute settings = GetListDrawerSettingsAttribute();
            Func<(int min, int max)> configuredLimits = SerializedListUtils.CreateSizeLimits(
                property, FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault(),
                FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);
            _element = new SerializedListElement(property, elementType, property.displayName, settings,
                (item, index) => CreateListItem(item, attributes, index),
                SerializedListUtils.CreateExtraSearch(property, elementType, FieldWithInfo.Targets[0], settings.ExtraSearch),
                () => configuredLimits?.Invoke() ?? _sizeLimits);

            void Search(string text) => UIToolkitUtils.SetDisplayStyle(_element,
                Util.UnityDefaultSimpleSearch(property.displayName, text) ? DisplayStyle.Flex : DisplayStyle.None);
            OnSearchFieldUIToolkit.AddListener(Search);
            _element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                OnSearchFieldUIToolkit.RemoveListener(Search);
                OnSearchFieldUIToolkit.AddListener(Search);
            });
            _element.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));
            return (_element, false);
        }

        private VisualElement CreateListItem(SerializedProperty prop, PropertyAttribute[] allAttributes, int index)
        {
            VisualElement resultField = UIToolkitUtils.CreateOrUpdateFieldProperty(
                prop,
                allAttributes,
                ReflectUtils.GetElementType(FieldWithInfo.FieldInfo.FieldType),
                $"Element {index}",
                FieldWithInfo.FieldInfo,
                InAnyHorizontalLayout,
                this,
                this,
                this,
                null,
                false,
                FieldWithInfo.Targets[0]
            );
            return resultField;
        }

        public override void OnDestroyUIToolkit()
        {
            _element?.StopSearch();
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);
            _sizeLimits = result.ArraySize;
            _element?.UpdateSizeLimits();
            return result;
        }
    }
}
#endif
