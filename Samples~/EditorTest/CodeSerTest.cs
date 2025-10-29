#if UNITY_EDITOR
using System.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Testing;

namespace SaintsField.Samples.EditorTest
{
    public class CodeSerTest : SaintsMonoBehaviour
    {

#if SAINTSFIELD_NEWTONSOFT_JSON
        [Button]
        private SerializedInfo[] T()
        {
            return SaintsEditorUtils.GetSaintsSerialized(typeof(SerEnumULong)).ToArray();
        }
#endif
    }
}
#endif
