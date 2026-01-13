using System.Linq;
using TMPro;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue355
{
    public class YellowWallPaperSpawner : SaintsMonoBehaviour
    {
        [GetPrefabWithComponent(typeof(YellowWallPaperDictionaryDupBuild))]
        public GameObject prefab;

        [SerializeField, GetComponent] private TMP_Text _tmpText;

        private void Start()
        {
#if AYELLOWPAPER_SERIALIZEDCOLLECTIONS
            YellowWallPaperDictionaryDupBuild r = Instantiate(prefab).GetComponent<YellowWallPaperDictionaryDupBuild>();
            _tmpText.text = string.Join("\n", r._rarities.Select(each => $"{each.Key}: {each.Value}"));
#endif
        }
    }
}
