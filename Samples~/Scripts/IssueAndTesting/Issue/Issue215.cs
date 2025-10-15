using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue215 : SaintsMonoBehaviour
    {
        // [BelowRichLabel("<field />")]
        // public float f;
        //
        // [BelowRichLabel("<field />")]
        // public double d;

        [FieldBelowText("<field />")]
        public float[] fs;

        [FieldBelowText("<field />")]
        public double[] ds;


        [ShowInInspector] private float[] _privateFs = new float[2];
        [ShowInInspector] private double[] _privateDs = new double[2];
    }
}
