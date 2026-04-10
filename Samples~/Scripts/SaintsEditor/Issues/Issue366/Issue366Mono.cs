using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue366
{
    public class Issue366Mono: SaintsMonoBehaviour
    {
        [LayoutStart("Spawner", ELayout.FoldoutBox | ELayout.Collapse)]
        [SaintsRow(true)]
        public SwarmSpawner_BuildObjectData swarmSpawner = new SwarmSpawner_BuildObjectData();
    }
}
