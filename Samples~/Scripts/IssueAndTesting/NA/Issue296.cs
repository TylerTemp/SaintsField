using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue296 : MonoBehaviour
    {
        [
            SerializeField,
            AssetPreview,
            ValidateInput(nameof(RequiredIfReloadable)),
        ] private Sprite ammoIcon = null;

        [
            SerializeField,
            Space,
#if SAINTSFIELD_SAMPLE_NAUGHYTATTRIBUTES
            ValidateInput(nameof(RequiredIfReloadable)),
            RichLabel("<label/>"),
            NaughtyAttributes.ShowAssetPreview,
            NaughtyAttributes.Label(" "),
#else
            InfoBox("NaughtyAttributes not installed", above: true),
#endif
        ]
        private Sprite naLabel = null;

        bool RequiredIfReloadable(Object o) => o != null;
    }
}
