#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public partial class TableRenderer
    {
        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, string label, Type valueType,
            object rawListValue, object[] listValue, Action<object> beforeSet, Action<object> setterOrNull,
            bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes,
            IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider, string foldoutViewKey)
        {
            if (oldElement is TableValueEditElement table && table.ClassListContains(foldoutViewKey))
            {
                table.Refresh(label, rawListValue, listValue, beforeSet, setterOrNull, targets);
                return null;
            }

            TableValueEditElement result = new TableValueEditElement(label, valueType, rawListValue, listValue,
                beforeSet, setterOrNull, labelGrayColor, allAttributes, targets, richTextTagProvider, foldoutViewKey);
            result.AddToClassList(foldoutViewKey);
            return result;
        }
    }
}
#endif
