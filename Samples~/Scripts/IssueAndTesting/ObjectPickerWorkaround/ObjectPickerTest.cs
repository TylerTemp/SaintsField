using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.ObjectPickerWorkaround
{
    public class ObjectPickerTest : MonoBehaviour
    {
        public GameObject objDefaultPicker;

        [TestObjectPicker]
        public string objectPicker;
    }
}
