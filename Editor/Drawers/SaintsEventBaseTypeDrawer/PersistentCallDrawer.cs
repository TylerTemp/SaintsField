#if SAINTSFIELD_SERIALIZATION && SAINTSFIELD_SERIALIZATION_ENABLE
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Events;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsEventBaseTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(PersistentCall), true)]
    public partial class PersistentCallDrawer: SaintsPropertyDrawer
    {
        private static string PropNameCallState() => nameof(PersistentCall.callState);
        private const string PropNameIsStatic = "_isStatic";
        private const string PropNameTarget = "_target";
        private const string PropNameTypeNameAndAssmble = "_staticType._typeNameAndAssembly";
        private const string PropMethodName = "_methodName";

        private static string GetDropPath(MethodInfo methodInfo, Type assType, string assShort, bool isImGui)
        {
            return isImGui
                ? $"{methodInfo.Name} {assType.Name}.{assType.Namespace}({assShort})"
                : $"{methodInfo.Name} <color=#808080>{assType.Name}.{assType.Namespace}({assShort})</color>";
        }

        private static AdvancedDropdownMetaInfo GetTypeDropdownMeta(Type curType, List<TypeDropdownGroup> typeDropdownGroups)
        {
            AdvancedDropdownList<TypeDropdownInfo> dropdownListValue = new AdvancedDropdownList<TypeDropdownInfo>
            {
                { "[Null]", default },
            };

            TypeDropdownInfo curSelected = default;
            bool hasSelected = false;

            foreach (TypeDropdownGroup typeDropdownGroup in typeDropdownGroups)
            {
                if(!string.IsNullOrEmpty(typeDropdownGroup.GroupName))
                {
                    dropdownListValue.AddSeparator();
                    dropdownListValue.Add(typeDropdownGroup.GroupName, default, true);
                    dropdownListValue.AddSeparator();
                }

                foreach (TypeDropdownInfo typeDropdownInfo in typeDropdownGroup.Types)
                {
                    dropdownListValue.Add(typeDropdownInfo.DropPath, typeDropdownInfo);

                    if (typeDropdownInfo.Type == curType)
                    {
                        curSelected = typeDropdownInfo;
                        hasSelected = true;
                    }
                }
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                CurDisplay = "",
                CurValues = hasSelected? new object[]{curSelected} :new object[]{},
                DropdownListValue = dropdownListValue,
                SelectStacks = new AdvancedDropdownAttributeDrawer.SelectStack[]
                {

                },
            };
        }

        private static AdvancedDropdownMetaInfo GetMethodDropdownMeta(string curMethodName, IEnumerable<MethodInfo> methodInfos)
        {
            AdvancedDropdownList<MethodInfo> dropdownListValue = new AdvancedDropdownList<MethodInfo>
            {
                { "[Null]", null },
            };
            dropdownListValue.AddSeparator();

            MethodInfo curSelected = null;

            foreach (MethodInfo methodInfo in methodInfos)
            {
                dropdownListValue.Add(methodInfo.Name, methodInfo);

                if (methodInfo.Name == curMethodName)
                {
                    curSelected = methodInfo;
                }
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                CurDisplay = "",
                CurValues = curSelected == null? new object[]{} :new object[]{curSelected},
                DropdownListValue = dropdownListValue,
                SelectStacks = new AdvancedDropdownAttributeDrawer.SelectStack[]
                {
                },
            };
        }
    }
}
#endif
