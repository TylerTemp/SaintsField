#if SAINTSFIELD_SERIALIZATION && !SAINTSFIELD_SERIALIZATION_DISABLED
using System;
using System.Collections.Generic;
using System.Linq;
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
        // private const string PropNameIsStatic = "_isStatic";
        // private const string PropNameTarget = "_target";
        // private const string PropNameTypeNameAndAssmble = "_staticType._typeNameAndAssembly";
        private const string SubPropNameTypeNameAndAssmble = "._typeNameAndAssembly";
        // private const string PropMethodName = "_methodName";
        // private const string PropNamePersistentArguments = "_persistentArguments";
        // private const string PropNamePersistentArgumentsTypeReferenceNameAndAssmbly = nameof(PersistentArgument.typeReference) + "._typeNameAndAssembly";
        // private const string PropNameReturnTypeNameAndAssmbly = "_returnType._typeNameAndAssembly";
        // private const string PropNamePersistentArgumentIsOptional = nameof(PersistentArgument.isOptional);
        // private const string PropNamePersistentArgumentNameAndAssmbly = nameof(PersistentArgument.typeReference) + "._typeNameAndAssembly";

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

        private static AdvancedDropdownMetaInfo GetMethodDropdownMeta(MethodSelect curMethod, IEnumerable<MethodSelect> methodInfos, bool isImGui)
        {
            AdvancedDropdownList<MethodSelect> dropdownListValue = new AdvancedDropdownList<MethodSelect>
            {
                { "[Null]", default },
            };

            Type preType = null;

            foreach (MethodSelect methodSelect in methodInfos)
            {
                if (methodSelect.Type != preType)
                {
                    preType = methodSelect.Type;
                    dropdownListValue.AddSeparator();
                    string typeName = methodSelect.Type.Name;
                    string typeNamespace = methodSelect.Type.Namespace;
                    string typeShort = TypeReference.GetShortAssemblyName(methodSelect.Type.Assembly);
                    string typePath = isImGui
                        ? $"{typeName}.{typeNamespace}({typeShort})"
                        : $"{typeName}<color=#add8e6>.{typeNamespace}({typeShort})</color>";
                    dropdownListValue.Add(typePath, default, true);
                    dropdownListValue.AddSeparator();
                }

                MethodInfo methodInfo = methodSelect.MethodInfo;

                string methodPath = StringifyMethod(
                    methodInfo.Name,
                    methodInfo.GetParameters().Select(each => each.ParameterType),
                    methodInfo.ReturnType);

                dropdownListValue.Add(methodPath, methodSelect);
            }

            return new AdvancedDropdownMetaInfo
            {
                Error = "",
                // FieldInfo = field,
                CurDisplay = "",
                CurValues = curMethod.MethodInfo == null? new object[]{} :new object[]{curMethod},
                DropdownListValue = dropdownListValue,
                SelectStacks = new AdvancedDropdownAttributeDrawer.SelectStack[]
                {
                },
            };
        }

        private static string StringifyMethod(string methodName, IEnumerable<Type> methodParams, Type returnType)
        {
            return $"{methodName}({string.Join(", ", methodParams.Select(SaintsEventUtils.StringifyType))}){(returnType == typeof(void)? "": $" => {SaintsEventUtils.StringifyType(returnType)}")}";
        }

        private static (bool isValidMethodInfo, IReadOnlyList<Type> paramTypes, Type returnType) GetMethodParamsType(SerializedProperty property)
        {
            SerializedProperty propPersistentArgumentsArray =
                property.FindPropertyRelative(nameof(PersistentCall.persistentArguments));
            List<Type> persistentArgumentTypes = new List<Type>();
            for (int index = 0; index < propPersistentArgumentsArray.arraySize; index++)
            {
                SerializedProperty eachPropParam = propPersistentArgumentsArray.GetArrayElementAtIndex(index);
                string typeName = eachPropParam.FindPropertyRelative(nameof(PersistentArgument.typeReference) + SubPropNameTypeNameAndAssmble).stringValue;
                if (string.IsNullOrEmpty(typeName))
                {
                    return (false, Array.Empty<Type>(), null);
                }

                Type persistentArgumentType = Type.GetType(typeName);
                if (persistentArgumentType == null)
                {
                    return (false, Array.Empty<Type>(), null);
                }

                persistentArgumentTypes.Add(persistentArgumentType);
            }

            SerializedProperty returnTypeProp = property.FindPropertyRelative(nameof(PersistentCall.returnType) + SubPropNameTypeNameAndAssmble);
            Type returnType = Type.GetType(returnTypeProp.stringValue);

            if (returnType == null)
            {
                return (false, Array.Empty<Type>(), null);
            }

            return (true, persistentArgumentTypes, returnType);
        }
    }
}
#endif
