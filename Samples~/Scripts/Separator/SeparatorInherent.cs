namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorInherent : SeparatorParent
    {
        [Separator("End of <b><container.Type.BaseType/></b>")]
        [Separator("Start of <b><container.Type/></b>")]
        public string inherent;
    }
}
