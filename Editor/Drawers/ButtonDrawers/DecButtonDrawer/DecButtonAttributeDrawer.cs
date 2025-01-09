using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer
{
    public abstract partial class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, object result) CallButtonFunc(SerializedProperty property, DecButtonAttribute decButtonAttribute, FieldInfo fieldInfo, object target)
        {
            return Util.GetMethodOf<object>(decButtonAttribute.FuncName, null, property, fieldInfo, target);
        }


    }
}
