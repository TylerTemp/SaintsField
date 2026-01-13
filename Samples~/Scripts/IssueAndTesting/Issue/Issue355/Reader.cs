using System.Linq;
using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue355
{
    public class Reader : SaintsMonoBehaviour
    {
        // [GetPrefabWithComponent(typeof(SaintsDictionaryDupBuild355))]
        public GameObject prefab;

        [SerializeField, GetComponent] private TMP_Text _tmpText;

        private void Start()
        {
            SaintsDictionaryDupBuild355 r = prefab.GetComponent<SaintsDictionaryDupBuild355>();
            _tmpText.text = string.Join("\n", r._rarities.Select(each => $"{each.Key}: {each.Value}"));
        }
    }
}
