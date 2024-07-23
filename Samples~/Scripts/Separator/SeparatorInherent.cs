namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorInherent : SeparatorParent
    {
        [Separator("End of <b><containerType.BaseType/></b>")]
        [Separator("Start of <b><containerType/></b>")]
        public string inherent;
    }
}
