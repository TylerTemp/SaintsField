using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Utils;
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
    }
}
