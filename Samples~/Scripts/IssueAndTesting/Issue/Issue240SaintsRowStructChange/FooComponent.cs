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
    }
}
