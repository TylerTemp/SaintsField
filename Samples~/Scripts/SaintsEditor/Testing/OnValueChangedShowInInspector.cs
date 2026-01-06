using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class OnValueChangedShowInInspector : SaintsMonoBehaviour
    {
        public interface IMyInterface
        {

        }

        public struct MyStruct : IMyInterface
        {
            public int StructInt;
            public override string ToString() => $"<MyStruct {StructInt}/>";
        }

        public class MyClass : IMyInterface
        {
            public int ClassInt;
            public override string ToString() => $"<MyClass {ClassInt}/>";
        }

        [OnValueChanged(nameof(ChangeCallback)), ShowInInspector]
        public IMyInterface myInterface;

        private void ChangeCallback(IMyInterface value)
        {
            Debug.Log(value);
        }
    }
}
