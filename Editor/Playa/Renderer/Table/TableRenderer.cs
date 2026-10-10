using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public partial class TableRenderer: SerializedFieldBaseRenderer
    {
        private readonly TableAttribute _attribute;

        public TableRenderer(TableAttribute attribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _attribute = attribute;
        }

        internal static bool SaintsFieldInfoShouldDraw(SaintsFieldWithInfo saintsFieldWithInfo)
        {
            return saintsFieldWithInfo.RenderType != SaintsRenderType.ClassStruct &&
                   SaintsEditor.SaintsFieldInfoShouldDraw(saintsFieldWithInfo);
        }

        private static int ChangeArraySize(int newValue, SerializedProperty arrayProp)
        {
            newValue = Mathf.Max(0, newValue);

            int oldValue = arrayProp.arraySize;
            if (newValue == oldValue)
            {
                return oldValue;
            }

            arrayProp.arraySize = newValue;

            if (newValue > oldValue && arrayProp.arraySize > 0)
            {
                if (arrayProp.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.ObjectReference)
                {
                    foreach (int index in Enumerable.Range(oldValue, newValue - oldValue))
                    {
                        arrayProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
                    }
                }
            }

            arrayProp.serializedObject.ApplyModifiedProperties();
            return newValue;
        }

        private static void DeleteArrayElement(SerializedProperty arrayProp, IEnumerable<int> selectedIndices)
        {
            if (arrayProp.arraySize == 0)
            {
                return;
            }

            List<int> indexes = selectedIndices.OrderByDescending(each => each).Where(each => each < arrayProp.arraySize).ToList();
            if (indexes.Count == 0)
            {
                indexes.Add(arrayProp.arraySize - 1);
            }

            foreach (int index in indexes)
            {
                int oldSize = arrayProp.arraySize;
                arrayProp.DeleteArrayElementAtIndex(index);
                if (arrayProp.arraySize == oldSize)
                {
                    arrayProp.DeleteArrayElementAtIndex(index);
                }
            }

            arrayProp.serializedObject.ApplyModifiedProperties();
        }
    }
}
