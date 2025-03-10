using SaintsField.Spine;
using Spine.Unity;
using UnityEngine;

namespace SaintsField.Samples.Spine.Scripts
{
    public class SpineAttachmentExample : MonoBehaviour
    {
        [SpineAttachment] public string attachmentOriginal;
        [SpineAttachmentPicker] public string spineAttachment;
    }
}
