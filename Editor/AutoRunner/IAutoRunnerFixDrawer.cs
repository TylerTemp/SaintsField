using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.AutoRunner
{
    public interface IAutoRunnerFixDrawer
    {
        AutoRunnerFixerResult AutoRunFix(SerializedProperty property, MemberInfo memberInfo, object parent);
    }
}
