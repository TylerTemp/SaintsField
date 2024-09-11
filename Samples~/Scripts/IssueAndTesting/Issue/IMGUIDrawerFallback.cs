using UnityEngine;
#if AYELLOWPAPER_SERIALIZEDCOLLECTIONS
using AYellowpaper.SerializedCollections;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class IMGUIDrawerFallback : MonoBehaviour
    {
#if AYELLOWPAPER_SERIALIZEDCOLLECTIONS
        public SerializedDictionary<string, int> serDic;

        [RichLabel("Fallback Drawer!")]
        public SerializedDictionary<string, int> serDicLabel;
#endif
    }
}
