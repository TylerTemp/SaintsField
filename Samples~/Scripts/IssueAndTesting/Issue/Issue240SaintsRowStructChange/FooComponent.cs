using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue240SaintsRowStructChange
{
    public class FooComponent : SaintsMonoBehaviour
    {
        public Foo1 foo1;

        [Serializable]
        public struct Foo1
        {
            [AboveButton(nameof(FooInline))]
            public int fooInline;

            [ShowInInspector]
            private int _fooNonSerInline;

            private void FooInline()
            {
                fooInline++;
                _fooNonSerInline++;
            }

            [Button]
            private void IncrementFoo()
            {
                fooButton++;
                _fooNonSerButton++;
            }

            public int fooButton;
            [ShowInInspector]
            private int _fooNonSerButton;

            public override string ToString()
            {
                return $"<Foo1 {fooInline}, {fooButton}>";
            }
        }
    }
}
