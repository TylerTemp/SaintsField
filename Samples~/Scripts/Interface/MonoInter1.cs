using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class MonoInter1: MonoBehaviour, IInterface1
    {
        public void Method1()
        {
            Debug.Log("Method1 in MonoInter1");
        }
    }
}
