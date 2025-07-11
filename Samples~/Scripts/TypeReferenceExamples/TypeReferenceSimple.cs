using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.TypeReferenceExamples
{
    public class TypeReferenceSimple : MonoBehaviour
    {
        // default using current & referenced assembly
        public TypeReference typeReference;

        // current assembly, and group it
        [TypeReference(EType.CurrentOnly | EType.GroupAssmbly | EType.GroupNameSpace)]
        [BelowButton(nameof(TestCreate))]
        public TypeReference typeReference2;

        private void TestCreate(TypeReference tr)
        {
            // you can also use `tr.Type`
            object t = Activator.CreateInstance(tr);
            Debug.Log(t);
        }

        // all assembly with non-public types, and group it
        [TypeReference(EType.AllAssembly | EType.AllowInternal | EType.GroupAssmbly)]
        public TypeReference typeReference3;

        public interface IMyTypeRef {}
        private struct MyTypeStruct: IMyTypeRef{}
        private class MyTypeClass : IMyTypeRef{}

        // Only types that implement IMyTypeRef
        [TypeReference(EType.AllAssembly | EType.AllowInternal, superTypes: new[]{typeof(IMyTypeRef)})]
        public TypeReference typeReferenceOf;
    }
}
