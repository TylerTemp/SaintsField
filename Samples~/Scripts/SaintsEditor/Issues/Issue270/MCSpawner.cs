using System.Collections;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue270
{
    public class MCSpawner : MCUnit
    {
        [Button]
        public void BatchSpawn(MCUnit target) => StartCoroutine(BatchSpawn_C(target));

        private IEnumerator BatchSpawn_C(MCUnit target)
        {
            yield return null;
        }
    }
}
