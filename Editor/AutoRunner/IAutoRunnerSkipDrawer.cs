using System.Reflection;
using UnityEditor;

namespace SaintsField.Editor.AutoRunner
{
    public interface IAutoRunnerSkipDrawer
    {
        bool AutoRunnerSkip(SerializedProperty property, MemberInfo memberInfo, object parent);
    }
}
