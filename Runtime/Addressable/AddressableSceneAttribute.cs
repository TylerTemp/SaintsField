namespace SaintsField.Addressable
{
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
