#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.BaseWrapTypeDrawer
{
    public partial class BaseWrapDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            InHorizontalLayout = true;

            (SerializedProperty realProp, FieldInfo realInfo) = GetBasicInfo(property, info);

            // (string getValueError, int _, object thisContainer) = Util.GetValue(property, info, parent);
            // Debug.Log($"thisContainer={}/{thisContainer.GetType()}");
            // Debug.Log(thisContainer);
            // Debug.Log(thisContainer.GetType());

            PropertyAttribute[] fieldAttributes = ReflectCache.GetCustomAttributes<PropertyAttribute>(realInfo);

            // foreach (PropertyAttribute fieldAttribute in fieldAttributes)
            // {
            //     if (fieldAttribute is ISaintsAttribute)
            //     {
            //
            //     }
            // }


            // Debug.Log($"{property.propertyPath}: {string.Join(",", allAttributes)}");
            // IReadOnlyList<PropertyAttribute> mergedAttributes = allAttributes.Concat(fieldAttributes).ToArray();

            // Debug.Log(parent.GetType());

            // VisualElement r = null;
            VisualElement r =
                UIToolkitUtils.CreateOrUpdateFieldProperty(
                    realProp,
                    fieldAttributes,
                    realInfo.FieldType,
                    null,
                    realInfo,
                    true,
                    this,
                    this,
                    null,
                    parent
                );
            // ReSharper disable once InvertIf
            if (r != null)
            {
                r.style.width = new StyleLength(Length.Percent(100));
                return r;
            }

            return new HelpBox($"Failed to render {property.propertyPath}, please report this issue", HelpBoxMessageType.Error);

            // // Debug.Log($"mergedAttributes={string.Join(" ", mergedAttributes)}");
            //
            // (PropertyAttribute[] _, object realParent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(realProp);
            //
            // // UnityFallbackUIToolkit(FieldInfo info, SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement containerElement, string passedPreferredLabel, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, object parent)
            //
            // using(new SaintsRowAttributeDrawer.ForceInlineScoop(true))
            // {
            //     return UnityFallbackUIToolkit(
            //         realInfo, realProp,
            //         mergedAttributes,
            //         container, "", SaintsPropertyDrawers,
            //         realParent);
            // }
            // // return PropertyFieldFallbackUIToolkit(realProp);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
        }
    }
}
#endif
