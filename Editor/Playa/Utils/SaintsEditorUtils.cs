using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using SaintsField.SaintsSerialization;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Utils
{
    public static class SaintsEditorUtils
    {
#if SAINTSFIELD_SERIALIZED && SAINTSFIELD_NEWTONSOFT_JSON
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            // ReSharper disable once ConvertToUsingDeclaration
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                StringBuilder sb = new StringBuilder();
                foreach (byte hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }
                return sb.ToString();
            }
        }
#endif

        public static ToggleCheckInfo FillResult(ToggleCheckInfo toggleCheckInfo, SerializedProperty serializedProperty)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(toggleCheckInfo.ConditionInfos, serializedProperty, null, toggleCheckInfo.Target);

            return new ToggleCheckInfo(toggleCheckInfo, errors, boolResults);
        }

        public static (bool show, bool disable) GetToggleResult(List<ToggleCheckInfo> toggleCheckInfos)
        {
            if (!toggleCheckInfos.TrueForAll((each) => each.Errors.Count == 0))
            {
                return (true, false);
            }

            List<bool> showResults = new List<bool>();
            // bool hide = false;
            // no disable attribute: not-disable
            // any disable attribute is true: disable; otherwise: not-disable
            bool disable = false;
            // no enable attribute: enable
            // any enable attribute is true: enable; otherwise: not-enable
            bool enable = true;

            foreach (ToggleCheckInfo toggleCheckInfo in toggleCheckInfos)
            {
                if (toggleCheckInfo.Errors.Count != 0)
                {
                    continue;
                }

                switch (toggleCheckInfo.Type)
                {
                    case ToggleType.Show:
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
                        Debug.Log(
                            $"show, count={toggleCheckInfo.boolResults.Count}, values={string.Join(",", toggleCheckInfo.boolResults)}");
#endif
                        showResults.Add(toggleCheckInfo.BoolResults.All(each => each));
                    }
                        break;
                    case ToggleType.Hide:
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
                        Debug.Log(
                            $"hide, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif

                        // Any(empty)=false=!hide=show. But because in ShowIf, empty=true=show, so we need to negate it.
                        if (toggleCheckInfo.BoolResults.Count == 0)
                        {
                            showResults.Add(false);  // don't show
                        }
                        else
                        {
                            bool willHide = toggleCheckInfo.BoolResults.Any(each => each);
                            showResults.Add(!willHide);
                        }
                    }
                        break;
                    case ToggleType.Disable:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
                        Debug.Log(
                            $"disable, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif
                        if (toggleCheckInfo.BoolResults.All(each => each))
                        {
                            disable = true;
                        }
                        break;
                    case ToggleType.Enable:
                        if (toggleCheckInfo.BoolResults.Count == 0)
                        {
                            // nothing means enable it or ignore
                        }
                        else
                        {
                            if (!toggleCheckInfo.BoolResults.Any(each => each))
                            {
                                enable = false;
                            }
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
                        Debug.Log(
                            $"enable={enable}, count={toggleCheckInfo.BoolResults.Count}, values={string.Join(",", toggleCheckInfo.BoolResults)}");
#endif
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(toggleCheckInfo.Type), toggleCheckInfo.Type, null);
                }
            }

            bool showIfResult = showResults.Count == 0 || showResults.Any(each => each);
            bool disableIfResult = disable || !enable;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
            Debug.Log(
                $"showIfResult={showIfResult} (hasShow={hasShow}, show={show}, hide={hide})");
#endif
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
            Debug.Log(
                $"disableIfResult={disableIfResult} (disable={disable}, enable={enable})");
#endif

            return (showIfResult, disableIfResult);
        }

        public static IReadOnlyList<SerializedInfo> GetSaintsSerialized(Type monoClass)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypesFromType(monoClass);

            List<SerializedInfo> results = new List<SerializedInfo>();

            // Debug.Log($"main={monoClass}; types={string.Join(", ", types)}");
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Type type in types)
            {
                // Debug.Log(type);
                MemberInfo[] memberLis = type
                    .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly);

                foreach (MemberInfo memberInfo in memberLis)
                {
                    if (memberInfo.Name.StartsWith("<") && memberInfo.Name.EndsWith(">k__BackingField"))
                    {
                        continue;
                    }

                    SerializedInfo result;
                    switch (memberInfo)
                    {
                        case FieldInfo fi:
                        {
                            result = GetSerializedInfo(fi, false, fi.FieldType);
                            // Debug.Log($"{fi.FieldType.IsArray}.{fi.FieldType}");
                        }
                            break;
                        case PropertyInfo pi:
                        {
                            result = GetSerializedInfo(pi, true, pi.PropertyType);
                            // Debug.Log($"{pi.PropertyType.IsArray}.{pi.PropertyType.IsArray}");
                        }
                            break;
                        default:
                            continue;
                    }

                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
            }

            // return results.Count > 0? (mainFilePath, results): default;
            return results;
        }

        private static SerializedInfo GetSerializedInfo(MemberInfo memberInfo, bool isProperty, Type targetType)
        {
            if (targetType == typeof(SaintsSerializedProperty))
            {
                return null;
            }

            Attribute[] attributes = ReflectCache.GetCustomAttributes<Attribute>(memberInfo);
            bool hasNonSer = false;
            bool hasSaintsSer = false;
            foreach (Attribute attribute in attributes)
            {
                switch (attribute)
                {
                    case NonSerializedAttribute _:
                        hasNonSer = true;
                        break;
                    case SaintsSerializedAttribute _:
                        hasSaintsSer = true;
                        break;
                }
            }

            if (hasNonSer && !hasSaintsSer)
            {
                return null;
            }

            SaintsTargetCollection targetCollection = SaintsTargetCollection.FieldOrProperty;
            Type elementType = null;
            if (targetType.IsArray)
            {
                targetCollection = SaintsTargetCollection.Array;
                elementType = targetType.GetElementType();
            }
            else
            {
                Type lisType = GetList(targetType);
                // ReSharper disable once InvertIf
                if (lisType != null)
                {
                    targetCollection = SaintsTargetCollection.List;
                    elementType = lisType.GetGenericArguments()[0];
                }
            }

            Type checkingType = targetCollection == SaintsTargetCollection.FieldOrProperty
                ? targetType
                : elementType;

            Debug.Assert(checkingType != null, targetType);

            // ReSharper disable once PossibleNullReferenceException
            bool checkingIsEnum = checkingType.IsEnum;

            // ReSharper disable once PossibleNullReferenceException
            string fillTypeNameWithNameSpace = checkingType.FullName.Replace('+', '.');

            if (!checkingIsEnum && IsGeneralClassOrStruct(checkingType))
            {
                if (!checkingType.IsDefined(typeof(SerializableAttribute)))
                {
                    return null;
                }

                SerializedInfo r = new SerializedInfo(memberInfo.Name, fillTypeNameWithNameSpace, isProperty, targetCollection, SaintsPropertyType.ClassOrStruct);

                // class/struct check
                foreach (MemberInfo subMember in checkingType
                             .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                         BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    SerializedInfo subSerInfo;
                    switch (subMember)
                    {
                        case FieldInfo fi:
                            subSerInfo = GetSerializedInfo(fi, false, fi.FieldType);
                            break;
                        case PropertyInfo pi:
                            subSerInfo = GetSerializedInfo(pi, true, pi.PropertyType);
                            break;
                        default:
                            continue;
                    }

                    if (subSerInfo != null)
                    {
                        r.SubFields.Add(subSerInfo);
                    }
                }

                return r.SubFields.Count > 0 ? r : null;
            }

            if (!checkingIsEnum)
            {
                return null;
            }

            Type underType = checkingType.GetEnumUnderlyingType();
            bool isLong = underType == typeof(long);
            if (isLong)
            {
                return new SerializedInfo(memberInfo.Name, fillTypeNameWithNameSpace, isProperty, targetCollection,
                    SaintsPropertyType.EnumLong);
            }

#if UNITY_2022_1_OR_NEWER
            bool isULong = underType == typeof(ulong);
            if (isULong)
            {
                return new SerializedInfo(memberInfo.Name, fillTypeNameWithNameSpace, isProperty, targetCollection,
                    SaintsPropertyType.EnumULong);
            }
#endif

            return null;
        }

        public static Type GetList(Type currentType)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(currentType))
            {
                return null;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Type baseType in ReflectUtils.GetGenBaseTypes(currentType))
            {
                if (baseType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return baseType;
                }
            }

            return null;
        }

        private static bool IsGeneralClassOrStruct(Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return false;
            }

            if (type == typeof(SaintsSerializedProperty))
            {
                return false;
            }

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum)
            {
                return false;
            }
            return type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum);
        }
    }
}
