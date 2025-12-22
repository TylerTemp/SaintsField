using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class WriterTest : MonoBehaviour
    {
        [Serializable]
        public struct MyStruct
        {
            public string myString;

            public void SetString()
            {
                Debug.Log("call func!");
                myString = "Hi Changed!";
            }
        }

        public MyStruct myStruct;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(WriterTest.MyStruct))]
    public class MyStructDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            root.Add(new PropertyField(property.FindPropertyRelative("myString")));
            root.Add(new Button(() =>
            {
                Object target = property.serializedObject.targetObject;
                FieldInfo field = typeof(WriterTest).GetField("myStruct");
                object fieldValue = field.GetValue(target);
                // Debug.Log(fieldValue);
                // Debug.Log(fieldValue.GetType());
                // var myStructField =

                MethodInfo func = typeof(WriterTest.MyStruct).GetMethod("SetString");
                func.Invoke(fieldValue, new object[] { });

                // This writes back the copied value
                field.SetValue(target, fieldValue);
            })
            {
                text = "change",
            });

            return root;
        }
    }
#endif
}
