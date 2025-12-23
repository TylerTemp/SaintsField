using System;
using System.Collections.Generic;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class StructValueChange : SaintsMonoBehaviour
    {
        public FooStruct foo1;
        public FooStruct[] foos;

        [Serializable]
        public struct FooStruct
        {
            [BelowButton(nameof(FooInline))]
            public int fooInline;

            private void FooInline()
            {
                fooInline++;
            }

            public int foo;

            [Button]
            private void IncrementFoo()
            {
                foo++;
            }

            public override string ToString()
            {
                return $"<Foo1 {fooInline}, {foo}>";
            }
        }

        [Serializable]
        public struct FloatWithBaseValue
        {
            [OnValueChanged((nameof(RevertFinalToBaseValue)))]
            public float baseValue;

            public float finalValue;

            public void RevertFinalToBaseValue()
            {
                finalValue = baseValue;
            }
        }

        public FloatWithBaseValue floatWithBaseValue;
        public FloatWithBaseValue[] floatWithBaseValues;


        [Serializable]
        public struct Nested
        {
            [ValidateInput(nameof(ValidateNested))]
            public int[] nestedValue;
            private string ValidateNested(int newValue, int index) => newValue < 0 ? $"Nested value at {index} should be positive, get {newValue}" : null;

            [ValidateInput(nameof(SameValueSet))]
            public int myValue;
            public int sameValue;

            private bool SameValueSet(int v)
            {
                sameValue = v;
                return true;
            }
        }

        public Nested nested;
        public List<Nested> nestedLis;

        [Serializable]
        public struct LabelRead
        {
            public int number;
            [LabelText("$" + nameof(number))]
            public string content;
        }

        public LabelRead labelRead;
        [FieldLabelText("<field.number/>")]
        public List<LabelRead> labelReadList;
    }
}
