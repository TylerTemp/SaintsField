using System.Collections.Generic;
using System.Reflection;
using SaintsField;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Samples.Scripts.SaintsEditor.Issues.Issue377.Editor
{
    [CustomPropertyDrawer(typeof(BVC))]
    public class BVCCusomEditor: SaintsRowAttributeDrawer
    {
        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement resultElement = new VisualElement();
            VisualElement core = base.CreateFieldUIToolKit(property, saintsAttribute, allAttributes, container, info, parent);
            resultElement.Add(core);

            resultElement.Add(new Button(() =>
            {
                object noCacheParent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                (string error, int index, object value) result = Util.GetValue(property, fieldInfo, noCacheParent);
                if (!string.IsNullOrEmpty(result.error))
                {
                    Debug.LogError(result.error);
                    return;
                }
                BVCWindow.OpenNew((BVC)result.value, property);
            })
            {
                text = "Edit",
            });

            return resultElement;
        }
    }
}
