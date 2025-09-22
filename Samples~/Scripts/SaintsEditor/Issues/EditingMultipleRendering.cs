using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class EditingMultipleRendering : SaintsMonoBehaviour
    {
        private struct MContainer
        {
            public string Name;
        }

        private struct CContainer
        {
            public string Name;
            public MContainer[] MContainers;
        }

        [Button, ShowInInspector]
        private CContainer[] Run2() => new[]
        {
            new CContainer
            {
                Name = "a",
                MContainers = new[]
                {
                    new MContainer { Name = "b" },
                    // new MContainer { Name = "c" },
                    // new MContainer { Name = "d" },
                }
            },
            // new CContainer
            // {
            //     Name = "b",
            //     MContainers = new[]
            //     {
            //         new MContainer { Name = "d" },
            //         // new MContainer { Name = "e" },
            //         // new MContainer { Name = "f" },
            //     }
            // },
        };
    }
}
