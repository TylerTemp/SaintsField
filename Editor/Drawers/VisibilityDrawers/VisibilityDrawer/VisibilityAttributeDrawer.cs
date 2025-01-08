using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.VisibilityDrawers.VisibilityDrawer
{
    public abstract partial class VisibilityAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerSkipDrawer
    {
        public bool AutoRunnerSkip(SerializedProperty property, MemberInfo memberInfo, object parent)
        {
        }
    }
}
