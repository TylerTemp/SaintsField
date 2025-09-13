using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ShowInInspectorMethodExample : SaintsMonoBehaviour
    {
        // A function is also supported
        [ShowInInspector]
        private string Function() => $"Function is supported ({Random.Range(0, 10)})";

        // Make a function like a real time calculator
        [ShowInInspector]
        private int AddCalculator(int a, int b)
        {
            return a + b;
        }

        [GetComponentInChildren] public Transform[] childrenTrans;

        private class ClassType
        {
            public Transform Value;
        }

        // class, struct and unity object are supported too
        [ShowInInspector]
        private ClassType GetClassType(int index) => new ClassType { Value = childrenTrans[(index % childrenTrans.Length + childrenTrans.Length) % childrenTrans.Length] };

        // [ShowInInspector]
        // private ClassType _showTrans;
        //
        // [Button]
        // private void Switch()
        // {
        //     // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        //     if(_showTrans?.Value != childrenTrans[0])
        //     {
        //         _showTrans = new ClassType { Value = childrenTrans[0] };
        //     }
        //     else
        //     {
        //         _showTrans = new ClassType { Value = childrenTrans[1] };
        //     }
        //     // Debug.Log(_showTrans);
        // }

    }
}
