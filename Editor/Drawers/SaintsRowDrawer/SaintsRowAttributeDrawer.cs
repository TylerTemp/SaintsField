using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsRowDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.AttributePriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsRowAttribute))]
    public partial class SaintsRowAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, int arrayIndex, object parent, object current) GetTargets(FieldInfo fieldInfo, SerializedProperty property)
        {
            object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            (string error, int index, object value) = Util.GetValue(property, fieldInfo, parentValue);
            if (error != "")
            {
                return (error, -1, parentValue, null);
            }

            return ("", index, parentValue, value);
        }

        public static IEnumerable<(string name, SerializedProperty property)> GetSerializableFieldInfo(SerializedProperty property)
        {
            HashSet<string> alreadySend = new HashSet<string>();
            SerializedProperty it = property.Copy();
            // or Next, also, the bool argument specifies whether to enter on children or not
            while (it.NextVisible(true))
            {
                // ReSharper disable once InvertIf
                if (alreadySend.Add(it.name))
                {
                    SerializedProperty relProperty = property.FindPropertyRelative(it.name);
                    // Debug.Log($"prop={it.name}/relProp={relProperty}");
                    if(relProperty != null)
                    {
                        yield return (it.name, relProperty);
                    }
                }
            }
        }

        // private IReadOnlyList<ISaintsRenderer> _imGuiRenderers;

        // private bool _testToggle;

// #if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
// #endif

        // public AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        // {
        //     return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        // }
    }
}
