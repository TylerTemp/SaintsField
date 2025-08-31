using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.TableDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(TableAttribute), true)]
    public partial class TableAttributeDrawer: SaintsPropertyDrawer
    {
        private struct SerializedPropertyInfo
        {
            public string Name;
            public string PropertyPath;
        }

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
            List<int> indexes = selectedIndices.OrderByDescending(each => each).Where(each => each < arrayProp.arraySize).ToList();
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

//         private int CompareProp(SerializedProperty a, SerializedProperty b)
//         {
//             switch (a.propertyType)
//             {
//                 case SerializedPropertyType.Generic:  // this can not be sorted
//                     return 0;
//                 case SerializedPropertyType.LayerMask:
//                 case SerializedPropertyType.Integer:
//                 case SerializedPropertyType.Enum:
//                     return a.intValue.CompareTo(b.intValue);
//                 case SerializedPropertyType.Boolean:
//                     return a.boolValue.CompareTo(b.boolValue);
//                 case SerializedPropertyType.Float:
//                     return a.floatValue.CompareTo(b.floatValue);
//                 case SerializedPropertyType.String:
//                     return string.Compare(a.stringValue, b.stringValue, StringComparison.Ordinal);
//                 case SerializedPropertyType.Color:
//                     return a.colorValue.grayscale.CompareTo(b.colorValue.grayscale);
//                 case SerializedPropertyType.ObjectReference:
//                     return string.Compare(a.objectReferenceValue?.name ?? "", b.objectReferenceValue?.name ?? "", StringComparison.Ordinal);
//                 case SerializedPropertyType.Vector2:
//                     return a.vector2Value.magnitude.CompareTo(b.vector2Value.magnitude);
// //                 case SerializedPropertyType.Vector3:
// //                     property.vector3Value = (Vector3) newValue;
// //                     break;
// //                 case SerializedPropertyType.Vector4:
// //                     property.vector4Value = (Vector4) newValue;
// //                     break;
// //                 case SerializedPropertyType.Rect:
// //                     property.rectValue = (Rect) newValue;
// //                     break;
// //                 case SerializedPropertyType.ArraySize:
// //                     property.arraySize = (int) newValue;
// //                     break;
// //                 case SerializedPropertyType.Character:
// //                     property.intValue = (char) newValue;
// //                     break;
// //                 case SerializedPropertyType.AnimationCurve:
// //                     property.animationCurveValue = (AnimationCurve) newValue;
// //                     break;
// //                 case SerializedPropertyType.Bounds:
// //                     property.boundsValue = (Bounds) newValue;
// //                     break;
// // #if UNITY_2022_2_OR_NEWER
// //                 case SerializedPropertyType.Gradient:
// //                     property.gradientValue = (Gradient) newValue;
// //                     break;
// // #endif
// //                 case SerializedPropertyType.Quaternion:
// //                     property.quaternionValue = (Quaternion) newValue;
// //                     break;
// //                 case SerializedPropertyType.ExposedReference:
// //                     property.exposedReferenceValue = (UnityEngine.Object) newValue;
// //                     break;
// //                 // case SerializedPropertyType.FixedBufferSize:  // this is readonly
// //                 //     property.fixedBufferSize = (int) curItem;
// //                 //     break;
// //                 case SerializedPropertyType.Vector2Int:
// //                     property.vector2IntValue = (Vector2Int) newValue;
// //                     break;
// //                 case SerializedPropertyType.Vector3Int:
// //                     property.vector3IntValue = (Vector3Int) newValue;
// //                     break;
// //                 case SerializedPropertyType.RectInt:
// //                     property.rectIntValue = (RectInt) newValue;
// //                     break;
// //                 case SerializedPropertyType.BoundsInt:
// //                     property.boundsIntValue = (BoundsInt) newValue;
// //                     break;
// // #if UNITY_2019_3_OR_NEWER
// //                 case SerializedPropertyType.ManagedReference:
// //                     property.managedReferenceValue = (UnityEngine.Object) newValue;
// //                     break;
// // #endif
// //                 // case SerializedPropertyType.Gradient:
// //                 case SerializedPropertyType.FixedBufferSize:
//                 default:
//                     return 0;
//                     // throw new ArgumentOutOfRangeException(nameof(a.propertyType), a.propertyType, null);
//             }
//         }
    }
}
