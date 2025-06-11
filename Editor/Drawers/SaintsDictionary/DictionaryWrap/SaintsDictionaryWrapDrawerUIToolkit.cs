#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SaintsDictionary.DictionaryWrap
{
    public partial class SaintsDictionaryWrapDrawer
    {
        protected override bool UseCreateFieldUIToolKit => true;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            InHorizontalLayout = true;

            (SerializedProperty realProp, FieldInfo realInfo) = GetBasicInfo(property, info);

            // Debug.Log($"{property.propertyPath}: {string.Join(",", allAttributes)}");
            // IReadOnlyList<PropertyAttribute> mergedAttributes = allAttributes.Concat(ReflectCache.GetCustomAttributes<PropertyAttribute>(realInfo)).ToArray();

            (PropertyAttribute[] _, object realParent) = SerializedUtils.GetAttributesAndDirectParent<PropertyAttribute>(realProp);

            // UnityFallbackUIToolkit(FieldInfo info, SerializedProperty property, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement containerElement, string passedPreferredLabel, IReadOnlyList<SaintsPropertyInfo> saintsPropertyDrawers, object parent)

            using(new SaintsRowAttributeDrawer.ForceInlineScoop(true))
            {
                return UnityFallbackUIToolkit(
                    realInfo, realProp, allAttributes, container, "", SaintsPropertyDrawers,
                    realParent);
            }
            // return PropertyFieldFallbackUIToolkit(realProp);
        }

        protected override void ChangeFieldLabelToUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, string labelOrNull,
            IReadOnlyList<RichTextDrawer.RichTextChunk> richTextChunks, bool tried, RichTextDrawer richTextDrawer)
        {
        }
    }
}
#endif
