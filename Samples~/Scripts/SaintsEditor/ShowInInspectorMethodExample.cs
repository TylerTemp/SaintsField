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

        [Separator(20)]
        // ReSharper disable InconsistentNaming

        [ShowInInspector] private Transform trans;

        [ShowInInspector]
        private Transform GetClassType(int index) =>
            childrenTrans[(index % childrenTrans.Length + childrenTrans.Length) % childrenTrans.Length];

        [Button]
        private void S() => trans = transform;

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
