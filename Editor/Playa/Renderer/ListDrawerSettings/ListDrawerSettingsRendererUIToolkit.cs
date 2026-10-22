#if UNITY_2021_3_OR_NEWER
using System;
using System.Linq;
using SaintsField.Editor.Drawers.SaintsWrapTypeDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.Utils;
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
            ListDrawerSettingsAttribute settings = GetListDrawerSettingsAttribute();
            Func<(int min, int max)> configuredLimits = SerializedListUtils.CreateSizeLimits(
                property, FieldWithInfo.PlayaAttributes.OfType<ArraySizeAttribute>().FirstOrDefault(),
                FieldWithInfo.FieldInfo, FieldWithInfo.Targets[0]);

            Attribute[] elementAttributes = ReflectCache
                .GetCustomAttributes<Attribute>(FieldWithInfo.FieldInfo)
                .Select(MapElementAttribute)
                .Where(each => each != null)
                .ToArray();

            _element = new SerializedListElement(property, elementType, property.displayName, settings,
                (item, index) => CreateListItem(item, elementAttributes, index),
                SerializedListUtils.CreateExtraSearch(property, elementType, FieldWithInfo.Targets[0], settings.ExtraSearch),
                () => configuredLimits?.Invoke() ?? _sizeLimits);

            OnSearchFieldUIToolkit.AddListener(Search);
            _element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                OnSearchFieldUIToolkit.RemoveListener(Search);
                OnSearchFieldUIToolkit.AddListener(Search);
            });
            _element.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));
            return (_element, false);

            void Search(string text) => UIToolkitUtils.SetDisplayStyle(_element,
                Util.UnityDefaultSimpleSearch(property.displayName, text) ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private static Attribute MapElementAttribute(Attribute attr)
        {
            switch (attr)
            {
                case PropertyAttribute pa:
                    return pa;
                case InjectAttributeBase injectAttribute:
                {
                    if (injectAttribute.Depth == 1)
                    {
                        Attribute injectedAttribute = SaintsWrapUtils.CreateInjectedAttribute(injectAttribute);
                        if (injectedAttribute is PropertyAttribute injectPa)
                        {
                            return injectPa;
                        }
#if SAINTSFIELD_DEBUG
                        Debug.LogWarning($"{injectAttribute} ignored from injection");
#endif
                    }
                    else if (injectAttribute.Depth > 1)
                    {
                        return new ValueAttributeAttribute(injectAttribute.Depth - 1, injectAttribute.Decorator,
                            injectAttribute.Parameters);
                    }

                    break;
                }
            }

            return null;
        }

        private VisualElement CreateListItem(SerializedProperty prop, Attribute[] allAttributes, int index)
        {
            VisualElement resultField = UIToolkitUtils.CreateOrUpdateFieldProperty(
                prop,
                allAttributes.Where(ListItemPropertyFilter).ToArray(),
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

        private static bool ListItemPropertyFilter(Attribute each)
        {
            if (each is IPlayaAttribute)
            {
                return each is PropertyAttribute;
            }

            return true;
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
