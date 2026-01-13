using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue355
{
    public class YellowWallPaperDictionaryDupBuild : MonoBehaviour
    {
#if AYELLOWPAPER_SERIALIZEDCOLLECTIONS
        public AYellowpaper.SerializedCollections.SerializedDictionary<Rarity, GameObject> _rarities;
#endif
    }
}
