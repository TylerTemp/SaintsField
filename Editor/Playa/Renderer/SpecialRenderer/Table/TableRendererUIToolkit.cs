#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Linq;
using System.Reflection;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.SpecialRenderer.Table
{
    public partial class TableRenderer
    {
        private static string NameTableContainer(SerializedProperty property)
        {
            return $"saints-table-container-{property.propertyPath}";
        }

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
            Debug.Assert(tableAttribute != null, FieldWithInfo.SerializedProperty.propertyPath);

            VisualElement result = new VisualElement
            {
                name = NameTableContainer(FieldWithInfo.SerializedProperty),
            };
            FillTable(FieldWithInfo.SerializedProperty, result);

            return (result, true);
        }

        private static void FillTable(SerializedProperty arrayProperty, VisualElement result)
        {
            bool hasSize = arrayProperty.arraySize > 0;
            SerializedProperty targetProperty = hasSize
                ? arrayProperty.GetArrayElementAtIndex(0)
                : arrayProperty;

            PropertyField propField = new PropertyField(targetProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            propField.Bind(arrayProperty.serializedObject);

            if (hasSize)
            {
                Foldout foldout = new Foldout
                {
                    value = true,
                    text = arrayProperty.displayName,
                    style = { flexGrow = 1 },
                };
                foldout.Add(propField);
                result.Add(foldout);
            }
            else
            {
                result.Add(propField);
            }
        }
    }
}
#endif
