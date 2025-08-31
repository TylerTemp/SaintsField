using System.Diagnostics;

namespace SaintsField.Addressable
{
    [Conditional("UNITY_EDITOR")]
    public class AddressableSceneAttribute: AddressableAddressAttribute
    {
        public readonly bool SepAsSub = true;

        public AddressableSceneAttribute(string group = null, params string[] orLabels): base(group, orLabels)
        {
        }

        public AddressableSceneAttribute(bool sepAsSub, string group = null, params string[] orLabels): this(group, orLabels)
        {
            SepAsSub = sepAsSub;
        }
    }
}
