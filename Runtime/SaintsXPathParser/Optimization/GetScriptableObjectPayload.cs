namespace SaintsField.SaintsXPathParser.Optimization
{
    public class GetScriptableObjectPayload: OptimizationPayload
    {
        public readonly string PathSuffix;

        public GetScriptableObjectPayload(string pathSuffix)
        {
            PathSuffix = pathSuffix;
        }
    }
}
