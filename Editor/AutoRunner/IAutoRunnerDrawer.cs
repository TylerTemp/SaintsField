using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.AutoRunner
{
    public interface IAutoRunnerDrawer
    {
        AutoRunnerFixerResult AutoRun(SerializedProperty property, MemberInfo memberInfo, object parent);
    }
}
