using SaintsField.Playa;


namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AroundInfoBox : SaintsMonoBehaviour
    {
        [LayoutStart("Main", ELayout.Horizontal | ELayout.TitleBox)]

        [PlayaInfoBox("This is the main section")]

        [LayoutStart("./1")]

        [Button("Method1")]
        private void M1(){}

        [LayoutStart("../2")]
        [Button("Method2")]
        private void M2(){}
    }
}
