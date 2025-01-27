using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.TableDrawer
{
    [CustomPropertyDrawer(typeof(TableAttribute))]
    public partial class TableAttributeDrawer: SaintsPropertyDrawer
    {

        private static int ChangeArraySize(int newValue, SerializedProperty arrayProp)
        {
            int oldValue = arrayProp.arraySize;
            if (newValue == oldValue)
            {
                return oldValue;
            }

            arrayProp.arraySize = newValue;

            if (newValue > oldValue)  // add
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
            List<int> indexes = selectedIndices.OrderByDescending(each => each).ToList();
            if (indexes.Count == 0)
            {
                indexes.Add(arrayProp.arraySize - 1);
            }

            foreach (int index in indexes)
            {
                arrayProp.DeleteArrayElementAtIndex(index);
            }

            arrayProp.serializedObject.ApplyModifiedProperties();
        }

    }
}
