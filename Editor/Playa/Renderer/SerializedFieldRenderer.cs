using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections;
using SaintsField.Editor.Drawers;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
        }

        private static void InvokeCallback(string callback, object newValue, object parent)
        {
            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(parent);
            types.Reverse();

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            foreach (Type type in types)
            {
                MethodInfo methodInfo = type.GetMethod(callback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), new[]
                {
                    newValue,
                });

                try
                {
                    methodInfo.Invoke(parent, passParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    // Debug.Assert(e.InnerException != null);
                    // return e.InnerException?.Message ?? e.Message;
                    return;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    // return e.Message;
                    return;
                }
                catch (Exception e)
                {
                    // _error = e.Message;
                    Debug.LogException(e);
                    // return e.Message;
                    return;
                }

                // return "";
                return;
            }

            string error = $"No field or method named `{callback}` found on `{parent}`";
            Debug.LogError(error);
        }

        private static void InvokeArraySizeCallback(string callback, SerializedProperty property, MemberInfo memberInfo)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning("Property disposed unexpectedly");
                return;
            }

            (string error, int _, object newValue) = Util.GetValue(property, memberInfo, parent);
            if (error != "")
            {
                Debug.LogError(error);
                return;
            }

            InvokeCallback(callback, newValue, parent);
        }

        private string _curXml;
        private RichTextDrawer.RichTextChunk[] _curXmlChunks;

        #region ListUtils

        private static IEnumerable<int> SearchArrayProperty(SerializedProperty property, string search)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (int index in Enumerable.Range(0, property.arraySize))
            {
                SerializedProperty childProperty = property.GetArrayElementAtIndex(index);
                if(SearchProp(childProperty, search))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"found: {childProperty.propertyPath}");
#endif
                    yield return index;
                }
            }
        }

        private static bool SearchProp(SerializedProperty property, string search)
        {
            SerializedPropertyType propertyType;
            try
            {
                propertyType = property.propertyType;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }

            // Debug.Log($"{property.propertyPath} is {propertyType}");

            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString().Contains(search);
                case SerializedPropertyType.Float:
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    return property.floatValue.ToString().Contains(search);
                case SerializedPropertyType.String:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                    Debug.Log($"{property.propertyPath}={property.stringValue} contains {search}={property.stringValue?.Contains(search)}");
#endif
                    return property.stringValue?.Contains(search) ?? false;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString().Contains(search);
                case SerializedPropertyType.ObjectReference:
                    // ReSharper disable once Unity.NoNullPropagation
                    if (property.objectReferenceValue is ScriptableObject so)
                    {
                        return SearchSoProp(so, search);
                    }
                    return property.objectReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.Enum:
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if(property.enumNames.Length <= property.enumValueIndex || property.enumValueIndex < 0)
                    {
                        return false;
                    }
                    return property.enumNames[property.enumValueIndex].Contains(search);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString().Contains(search);
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString().Contains(search);
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString().Contains(search);
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString().Contains(search);
                case SerializedPropertyType.ArraySize:
                    if (property.isArray)
                    {
                        return property.arraySize.ToString().Contains(search);
                    }
                    goto default;
                case SerializedPropertyType.Character:
                    return property.intValue.ToString().Contains(search);
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue.ToString().Contains(search);
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString().Contains(search);
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString().Contains(search);
                case SerializedPropertyType.ExposedReference:
                    // ReSharper disable once Unity.NoNullPropagation
                    return property.exposedReferenceValue?.name.Contains(search) ?? false;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString().Contains(search);
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString().Contains(search);
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString().Contains(search);
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString().Contains(search);
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString().Contains(search);
#if UNITY_2019_3_OR_NEWER
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename.Contains(search);
#endif
                case SerializedPropertyType.Generic:
                {
                    if (property.isArray)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                        Debug.Log($"is array {property.arraySize}: {property.propertyPath}");
#endif
                        return Enumerable.Range(0, property.arraySize)
                            .Select(property.GetArrayElementAtIndex)
                            .Any(childProperty => SearchProp(childProperty, search));
                    }

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (SerializedProperty child in SerializedUtils.GetPropertyChildren(property))
                    {
                        if(SearchProp(child, search))
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
                            Debug.Log($"found child: {child.propertyPath}");
#endif
                            return true;
                        }
                    }

                    return false;
                }
                case SerializedPropertyType.Gradient:
#if UNITY_2021_1_OR_NEWER
                case SerializedPropertyType.Hash128:
#endif
                default:
                    return false;
            }
        }

        private static bool SearchSoProp(ScriptableObject so, string search)
        {
            // ReSharper disable once ConvertToUsingDeclaration
            using(SerializedObject serializedObject = new SerializedObject(so))
            {
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (SearchProp(iterator, search))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private struct PagingInfo
        {
            public IReadOnlyList<int> IndexesAfterSearch;
            public List<int> IndexesCurPage;
            public int CurPageIndex;
            public int PageCount;
        }

        private static PagingInfo GetPagingInfo(SerializedProperty property, int newPageIndex, string search, int numberOfItemsPerPage)
        {
            IReadOnlyList<int> fullIndexResults = string.IsNullOrEmpty(search)
                ? Enumerable.Range(0, property.arraySize).ToList()
                : SearchArrayProperty(property, search).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"index search={search} result: {string.Join(",", fullIndexResults)}; numberOfItemsPerPage={numberOfItemsPerPage}");
#endif

            // List<int> itemIndexToPropertyIndex = fullIndexResults.ToList();
            int curPageIndex;

            int pageCount;
            int skipStart;
            int itemCount;
            if (numberOfItemsPerPage <= 0)
            {
                pageCount = 1;
                curPageIndex = 0;
                skipStart = 0;
                itemCount = int.MaxValue;
            }
            else
            {
                pageCount = Mathf.CeilToInt((float)fullIndexResults.Count / numberOfItemsPerPage);
                curPageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
                skipStart = curPageIndex * numberOfItemsPerPage;
                itemCount = numberOfItemsPerPage;
            }

            List<int> curPageItemIndexes = fullIndexResults.Skip(skipStart).Take(itemCount).ToList();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_LIST_DRAWER_SETTINGS
            Debug.Log($"set items: {string.Join(", ", curPageItemIndexes)}, itemIndexToPropertyIndex={string.Join(",", fullIndexResults)}");
#endif

            return new PagingInfo
            {
                IndexesAfterSearch = fullIndexResults,
                IndexesCurPage = curPageItemIndexes,
                CurPageIndex = curPageIndex,
                PageCount = pageCount,
            };
        }


        #endregion

        public override string ToString() => $"Ser<{FieldWithInfo.FieldInfo?.Name ?? FieldWithInfo.SerializedProperty.displayName}>";
    }
}
