using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    // [CreateAssetMenu(menuName = "SaintsField/Samples/ScriptableInter12")]
    public class ScriptableInter12: ScriptableObject, IInterface1, IInterface2, IDummy
    {
        [Range(0, 1)] public float defaultRange;

        [SepTitle(EColor.Gray)]
        [PropRange(0, 1)] public float propRange;

        public void Method1()
        {
            Debug.Log("ScriptableInter12 Method1");
        }

        public string GetComment() => name;
    }
}
