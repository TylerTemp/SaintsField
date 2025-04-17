using UnityEngine;
using TMPro;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround
{
    public class ObjectPickerTest : MonoBehaviour
    {
        public TMPro.TMP_FontAsset objDefaultPicker;

        [TestObjectPicker]
        public string objectPicker;
    }
}
