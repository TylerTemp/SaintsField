namespace SaintsField.Samples.Scripts.Separator
{
    public class SeparatorInherent : SeparatorParent
    {
        [FieldSeparator("End of <b><container.Type.BaseType/></b>")]
        [FieldSeparator("Start of <b><container.Type/></b>")]
        public string inherent;
    }
}
