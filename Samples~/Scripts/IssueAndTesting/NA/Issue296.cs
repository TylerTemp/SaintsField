using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.NA
{
    public class Issue296 : MonoBehaviour
    {
        [
            SerializeField,
            AssetPreview,
            ValidateInput(nameof(RequiredIfReloadable)),
            // ReSharper disable once NotAccessedField.Local
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
            InfoBox("NaughtyAttributes not installed"),
#endif
        ]
        // ReSharper disable once NotAccessedField.Local
#pragma warning disable 0296
        private Sprite naLabel = null;
#pragma warning restore 0296

        private bool RequiredIfReloadable(Object o) => o != null;
    }
}
