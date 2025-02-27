using System;
using System.Linq;
using System.Reflection;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer
{
    public partial class SerializedFieldBaseRenderer
    {
        private Texture2D _iconDown;
        private Texture2D _iconLeft;
        private Texture2D _iconRight;

        // ReSharper disable once MemberCanBePrivate.Global

        protected bool CheckArraySizeAttribute(PreCheckResult preCheckResult)
        {
            bool changed = false;
            (int min, int max) arraySize = preCheckResult.ArraySize;
            if(arraySize.min > 0 && FieldWithInfo.SerializedProperty.arraySize < arraySize.min)
            {
                changed = true;
                FieldWithInfo.SerializedProperty.arraySize = arraySize.min;
            }
            if(arraySize.max > 0 && FieldWithInfo.SerializedProperty.arraySize > arraySize.max)
            {
                changed = true;
                FieldWithInfo.SerializedProperty.arraySize = arraySize.max;
            }

            return changed;
        }

        protected static void DragAndDropImGui(Rect rect, Type elementType, SerializedProperty property)
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            if (!rect.Contains(evt.mousePosition))
            {
                return;
            }

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.None;

                DragAndDrop.AcceptDrag();

                Object[] acceptItems = CanDrop(DragAndDrop.objectReferences, elementType).ToArray();
                if (acceptItems.Length == 0)
                {
                    return;
                }

                int startIndex = property.arraySize;
                int totalCount = acceptItems.Length;
                property.arraySize += totalCount;
                foreach ((SerializedProperty prop, Object obj) in Enumerable.Range(startIndex, totalCount)
                             .Select(property.GetArrayElementAtIndex).Zip(acceptItems, (prop, obj) =>
                                 (prop, obj)))
                {
                    prop.objectReferenceValue = obj;
                }

                // foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                // {
                //     Debug.Log("Dropped object: " + draggedObject.name);
                // }

                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                DragAndDrop.visualMode = CanDrop(DragAndDrop.objectReferences, elementType).Any()
                    ? DragAndDropVisualMode.Copy
                    : DragAndDropVisualMode.Rejected;
            }

            Event.current.Use();
        }


        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            bool isArray = FieldWithInfo.SerializedProperty.isArray;
            OnArraySizeChangedAttribute onArraySizeChangedAttribute =
                FieldWithInfo.PlayaAttributes.OfType<OnArraySizeChangedAttribute>().FirstOrDefault();
            int arraySize = -1;
            if (isArray && onArraySizeChangedAttribute != null)
            {
                arraySize = FieldWithInfo.SerializedProperty.arraySize;
            }

            bool arraySizechanged = CheckArraySizeAttribute(preCheckResult);
            if (arraySizechanged)
            {
                FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif

                SerializedFieldRenderPositionTargetIMGUI(position, preCheckResult);

                if(changed.changed && isArray && onArraySizeChangedAttribute != null &&
                   (arraySizechanged || arraySize != FieldWithInfo.SerializedProperty.arraySize))
                {
                    FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    InvokeArraySizeCallback(onArraySizeChangedAttribute.Callback,
                        FieldWithInfo.SerializedProperty,
                        (MemberInfo)FieldWithInfo.FieldInfo ?? FieldWithInfo.PropertyInfo);
                }
            }
        }

        protected abstract void SerializedFieldRenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult);
    }
}
