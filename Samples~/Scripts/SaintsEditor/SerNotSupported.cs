using System;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public partial class SerNotSupported : SaintsMonoBehaviour
    {
        [SaintsSerialized] public int letUnitySerInt;
        [SaintsSerialized] public string letUnitySerString;

        // [Serializable]
        // public partial struct CustomType<T>
        // {
        //     [SaintsSerialized]
        //     public T value;
        // }
    }
}
