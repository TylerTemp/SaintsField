#if UNITY_2021_3_OR_NEWER //&& !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
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
            return $"saints-table-container-{SerializedUtils.GetUniqueId(property)}";
        }

        private bool _hasSize;

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            TableAttribute tableAttribute = FieldWithInfo.PlayaAttributes.OfType<TableAttribute>().FirstOrDefault();
            Debug.Assert(tableAttribute != null, FieldWithInfo.SerializedProperty.propertyPath);

            VisualElement result = new VisualElement
            {
                name = NameTableContainer(FieldWithInfo.SerializedProperty),
            };

            // Type elementType =
            //     ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
            //                                 FieldWithInfo.PropertyInfo.PropertyType);
            //
            // _hasSize = FillTable(FieldWithInfo.SerializedProperty, result, elementType, FieldWithInfo.SerializedProperty);
            FillTableToContainer(result);

            OnSearchFieldUIToolkit.AddListener(Search);
            result.RegisterCallback<DetachFromPanelEvent>(_ => OnSearchFieldUIToolkit.RemoveListener(Search));

            return (result, true);

            void Search(string search)
            {
                DisplayStyle display = Util.UnityDefaultSimpleSearch(FieldWithInfo.SerializedProperty.displayName, search)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (result.style.display != display)
                {
                    result.style.display = display;
                }
            }
        }

        private void FillTableToContainer(VisualElement container)
        {
            container.Clear();
            Type elementType =
                ReflectUtils.GetElementType(FieldWithInfo.FieldInfo?.FieldType ??
                                            FieldWithInfo.PropertyInfo.PropertyType);

            _hasSize = FillTable(FieldWithInfo.SerializedProperty, container, elementType, FieldWithInfo.SerializedProperty);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = base.OnUpdateUIToolKit(root);

            int curSize = FieldWithInfo.SerializedProperty.arraySize;
            // ReSharper disable once InvertIf
            if ((curSize > 0 && !_hasSize)
                || (curSize == 0 && _hasSize))
            {
                VisualElement tableContainer =
                    root.Q<VisualElement>(name: NameTableContainer(FieldWithInfo.SerializedProperty));
                // tableContainer.Clear();

                FillTableToContainer(tableContainer);
            }

            return result;
        }

        private static bool FillTable(SerializedProperty arrayProperty, VisualElement result, Type elementType, SerializedProperty property)
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
                UIToolkitUtils.AddContextualMenuManipulator(foldout, arrayProperty, () => {});
                foldout.Add(propField);
                result.Add(foldout);

                result.Add(new EmptyPrefabOverrideElement(arrayProperty)
                {
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        bottom = 0,
                        left = 0,
                        right = 0,
                        height = 18,
                    },
                    pickingMode = PickingMode.Ignore,
                });

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

            return hasSize;
        }
    }
}
#endif
