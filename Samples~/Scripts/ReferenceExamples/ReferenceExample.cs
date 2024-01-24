using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.ReferenceExamples
{
    public class ReferenceExample: MonoBehaviour
    {
        [Serializable]
        public class Base1Fruit
        {
            public GameObject imInBase1;
        }


        [Serializable]
        public class Base2Fruit: Base1Fruit
        {
            public int base2;
        }

        [Serializable]
        public class Apple : Base2Fruit
        {
            public string apple;
            public GameObject applePrefab;
        }

        [Serializable]
        public class Orange : Base2Fruit
        {
            public bool orange;
        }

        // Use SerializeReference if this field needs to hold both
        // Apples and Oranges.  Otherwise only m_Data from Base object would be serialized
        [SerializeReference, ReferencePicker]
        public Base2Fruit itemWithInitValue = new Apple();

        [SerializeReference, ReferencePicker]
        public Base2Fruit item2;
    }
}
