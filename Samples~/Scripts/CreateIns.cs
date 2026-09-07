namespace SaintsField.Samples.Scripts
{
    public class CreateIns : SaintsMonoBehaviour
    {
        [GetPrefabWithComponent, Expandable] public SaintsArray2DRBuildTest prefab;

        private void Awake()
        {
            Instantiate(prefab, transform);
        }
    }
}
