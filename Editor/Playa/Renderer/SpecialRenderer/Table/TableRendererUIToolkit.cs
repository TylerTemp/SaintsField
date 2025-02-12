#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using SaintsField.Editor.Utils;
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

            Type elementType =
                ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
                                            FieldWithInfo.PropertyInfo.PropertyType);

            FillTable(FieldWithInfo.SerializedProperty, result, elementType, FieldWithInfo.SerializedProperty);

            return (result, true);
        }

        private static void FillTable(SerializedProperty arrayProperty, VisualElement result, Type elementType, SerializedProperty property)
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

                #region Drag
                VisualElement foldoutInput = foldout.Q<VisualElement>(classes: "unity-foldout__input");

                foldoutInput.RegisterCallback<DragEnterEvent>(_ =>
                {
                    // Debug.Log($"Drag Enter {evt}");
                    DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                        ? DragAndDropVisualMode.Copy
                        : DragAndDropVisualMode.Rejected;
                });
                foldoutInput.RegisterCallback<DragLeaveEvent>(_ =>
                {
                    // Debug.Log($"Drag Leave {evt}");
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;
                });
                foldoutInput.RegisterCallback<DragUpdatedEvent>(_ =>
                {
                    // Debug.Log($"Drag Update {evt}");
                    // DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                        ? DragAndDropVisualMode.Copy
                        : DragAndDropVisualMode.Rejected;
                });
                foldoutInput.RegisterCallback<DragPerformEvent>(_ =>
                {
                    // Debug.Log($"Drag Perform {evt}");
                    if (!DropUIToolkit(elementType, property))
                    {
                        return;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                });
                #endregion
            }
            else
            {
                result.Add(propField);
            }
        }
    }
}
#endif
