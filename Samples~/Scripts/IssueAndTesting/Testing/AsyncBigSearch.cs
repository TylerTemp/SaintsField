using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class AsyncBigSearch : SaintsMonoBehaviour
    {
        [ListDrawerSettings(searchable: true, numberOfItemsPerPage: 20), GetByXPath("scene:://*")]
        public GameObject[] gameObjects;
    }
}
