using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue97 : SaintsMonoBehaviour
    {
        // [Serializable]
        // public class MyClass
        // {
        //     public string unique;
        // }
        //
        // // [OnValueChanged(nameof(ValueChanged))]
        // [OnChanged(nameof(Changed))]
        // public MyClass[] myClasses;
        //
        // public void ValueChanged(MyClass myClass, int index)
        // {
        //     Debug.Log($"OnValueChanged: {myClass.unique} at {index}");
        // }
        //
        // public void Changed(IReadOnlyList<MyClass> myClassNewValues)
        // {
        //     // foreach (MyClass newValue in myClassNewValues)
        //     // {
        //     //     Debug.Log(newValue?.unique);
        //     // }
        //     Debug.Log($"OnChanged: {string.Join("; ", myClassNewValues.Select(each => each?.unique))}");
        // }

#if UNITY_2021_3_OR_NEWER
        public interface IMyInterface
        {
        }

        [Serializable]
        public struct MyStructIn : IMyInterface
        {
            public int myInt;
            public override string ToString() => $"MyStructIn: {myInt}";
        }

        [Serializable]
        public class MyClassIn : IMyInterface
        {
            public string myString = "Default String";
            public override string ToString() => $"MyClassIn: {myString}";
        }

        // [SerializeReference, ReferencePicker, OnChanged(nameof(InterfaceValueChanged))]
        // public IMyInterface myInterface;
        //
        // public void InterfaceValueChanged(IMyInterface newValue)
        // {
        //     Debug.Log($"InterfaceValueChanged: {newValue}");
        // }

        [SerializeReference, ReferencePicker, OnChanged(nameof(InterfacesValueChanged))]
        public IMyInterface[] myInterfaces;

        public void InterfacesValueChanged(IEnumerable<IMyInterface> newValues)
        {
            Debug.Log($"InterfacesValueChanged: {string.Join(";", newValues)}");
        }
#endif
    }
}
