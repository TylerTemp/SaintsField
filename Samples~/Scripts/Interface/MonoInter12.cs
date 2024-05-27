using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class MonoInter12: MonoBehaviour, IInterface1, IInterface2
    {
        public void Method1()
        {
            Debug.Log("Method1 in MonoInter2");
        }
    }
}
