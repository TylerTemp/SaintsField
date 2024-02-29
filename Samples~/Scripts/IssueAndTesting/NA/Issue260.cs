using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue260 : MonoBehaviour
    {
        public bool isNull;

        [ShowInInspector] public const object NullValue = null;

        [ShowInInspector]
        public object ChangeableValue => isNull ? null : "Some String";

        // [ShowInInspector] public static readonly int[] ArrInts = {1, 2, 3, 4, 5};
    }
}
