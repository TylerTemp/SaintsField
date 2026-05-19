using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing.ShowInInspectorGenImprove
{
    public class ShowInInspectorInterfaceUnity : SaintsMonoBehaviour
    {
        private class MyObjClass : IObject
        {
            public string Str;
        }

        private class MyObjStruct : IObject
        {
            public string Str;
        }

        [ShowInInspector] private IObject _iObj;

        [ShowInInspector] private MyObjClass _myClass = new MyObjClass();
    }
}
