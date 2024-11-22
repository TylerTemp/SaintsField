using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OnValueChangedExample : MonoBehaviour
    {
        [OnValueChanged(nameof(Changed))]
        public int dir;

        private void Changed()
        {
            Debug.Log($"changed={dir}");
        }

        [OnValueChanged(nameof(ChangedParam)), InfoBox("$" + nameof(_belowText), EMessageType.Info, nameof(_belowText))]
        public int value;

        private string _belowText;

        private void ChangedParam(int newValue)
        {
            Debug.Log($"changed={newValue}");
            _belowText = $"changed={newValue}";
        }

        [OnValueChanged(nameof(ChangedAnyType))]
        public GameObject go;

        [OnValueChanged(nameof(ChangedAnyType))]
        public SpriteRenderer[] srs;

        private void ChangedAnyType(object anyObj, int index=-1)
        {
            Debug.Log($"changed={anyObj}@{index}");
        }

        [OnValueChanged(nameof(NumberChanged))]
        public int[] numbers;

        public string changedInfo;

        private void NumberChanged(int v, int index) => changedInfo = $"[{index}] changed to {v}";

        // serialized class change

        [Serializable]
        public struct MyStructInt
        {
            public int v;
        }

        [OnValueChanged(nameof(StructChanged))]
        public MyStructInt myStruct;

        private void StructChanged(MyStructInt m) => Debug.Log(m.v);

        // serialized class change list
        [OnValueChanged(nameof(StructChanged))]
        public MyStructInt[] myStructs;

#if UNITY_2021_3_OR_NEWER
        public interface IInt
        {
            public int V { get; }
        }

        [Serializable]
        public struct StructInter : IInt
        {
            public int v;
            public int V => v;
        }
        [Serializable]
        public class ClassInter : IInt
        {
            public int v = 10;
            public int V => v;
        }

        [SerializeReference, ReferencePicker, OnValueChanged(nameof(InterfaceChanged))]
        public IInt structInter;

        [SerializeReference, ReferencePicker, OnValueChanged(nameof(InterfaceChanged))]
        public IInt[] structInters;
        private void InterfaceChanged(IInt i) => Debug.Log(i.V);
#endif
    }
}
