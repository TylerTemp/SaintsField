using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.Optimization;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer
{
    public partial class GetByXPathAttributeDrawer
    {
        // private class GetXPathValueException : Exception
        // {
        //     public GetXPathValueException()
        //     {
        //     }
        //
        //     public GetXPathValueException(string message) : base(message)
        //     {
        //     }
        //
        //     public GetXPathValueException(string message, Exception innerException) : base(message, innerException)
        //     {
        //     }
        // }

        public class GetXPathValuesResult
        {
            public string XPathError;
            // ReSharper disable once NotAccessedField.Local
            public IEnumerable<object> Results;
        }

        public struct XPathResourceInfo
        {
            // ReSharper disable once NotAccessedField.Local
            public OptimizationPayload OptimizationPayload;
            public IReadOnlyList<GetByXPathAttribute.XPathInfo> OrXPathInfoList;
        }

        protected virtual GetXPathValuesResult GetXPathValues(IReadOnlyList<XPathResourceInfo> andXPathInfoList,
            Type expectedType, Type expectedInterface, SerializedProperty property, MemberInfo info, object parent)
        {
            return CalcXPathValues(andXPathInfoList, expectedType, expectedInterface, property, info, parent);
        }

        internal static GetXPathValuesResult CalcXPathValues(IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, MemberInfo info, object parent)
        {
            // Debug.Log($"andXPathInfoList Count={andXPathInfoList.Count}");
            bool anyResult = false;
            List<string> errors = new List<string>();
            // IEnumerable<object> finalResults = Array.Empty<object>();
            List<IEnumerable<object>> finalResultsCollected = new List<IEnumerable<object>>();

            foreach (XPathResourceInfo orXPathInfoList in andXPathInfoList)
            {
// #if SAINTSFIELD_AUTO_GETTER_RESOURCE_OPTIMIZE
                if (orXPathInfoList.OptimizationPayload != null)
                {
                    (string error, bool hasElement, IEnumerable<object> optimizedResult) = GetXPathByOptimized(orXPathInfoList.OptimizationPayload, property, info);
                    if (error == "")
                    {
                        if(hasElement)
                        {
                            anyResult = true;
                            finalResultsCollected.Add(optimizedResult
                                .Select(each => ValidateXPathResult(property.serializedObject.targetObject, each, expectedType, expectedInterface))
                                .Where(each => each.valid)
                                .Select(each => each.value));
                        }

                        continue;
                    }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                    Debug.Log($"#GetByXPath# OptimizationPayload failed: {error}");
#endif
                }
// #endif

                // Debug.Log($"loop andXPathInfoList");
                foreach (GetByXPathAttribute.XPathInfo xPathInfo in orXPathInfoList.OrXPathInfoList)
                {
                    IEnumerable<XPathStep> xPathSteps;
                    if (xPathInfo.IsCallback)
                    {
                        (string error, MemberInfo _, string xPathString) = Util.GetOf(xPathInfo.Callback, "", property, info, parent, null);

                        if (error != "")
                        {
                            errors.Add(error);
                            continue;
                        }

                        xPathSteps = XPathParser.Parse(xPathString);
                    }
                    else
                    {
                        xPathSteps = xPathInfo.XPathSteps;
                    }

                    IEnumerable<object> results = ResolveXPathValues(xPathSteps, orXPathInfoList.OptimizationPayload,
                        expectedType, expectedInterface, property, info, parent);
                    // if (results.Length != 0)
                    // {
                    //     finalResults.AddRange(results);
                    //     break;
                    // }
                    (bool hasElement, IEnumerable<object> elements) = Util.HasAnyElement(results);
                    if (hasElement)
                    {
                        anyResult = true;
                        // finalResults = finalResults.Concat(elements);
                        finalResultsCollected.Add(elements);
                        // Debug.Log($"has value, break on {finalResultsCollected.Count}");
                        break;
                    }
                }
            }

            // return (string.Join("\n", errors), Array.Empty<object>());

            return anyResult
                ? new GetXPathValuesResult
                {
                    XPathError = "",
                    Results = finalResultsCollected.SelectMany(each => each),
                }
                : new GetXPathValuesResult
                {
                    XPathError = string.Join("\n", errors),
                    Results = Array.Empty<object>(),
                };
        }

        internal static IEnumerable<object> ResolveXPathValues(IEnumerable<XPathStep> xPathSteps,
            OptimizationPayload optimizationPayload, Type expectedType, Type expectedInterface,
            SerializedProperty property, MemberInfo info, object parent)
        {
            if (optimizationPayload != null)
            {
                xPathSteps = PrepareShortcutSteps(xPathSteps, expectedType, expectedInterface);
            }
            IEnumerable<PathResolver.Node> accValues = PathResolver.Resolve(xPathSteps,
                PathResolver.CreateContext(property, info, parent), true);
            if (optimizationPayload is GetInSiblingsPayload)
            {
                GameObject self = property.serializedObject.targetObject is GameObject go ? go
                    : (property.serializedObject.targetObject as Component)?.gameObject;
                accValues = accValues.Where(node => (node.Resource is GameObject resultGo ? resultGo
                    : (node.Resource as Component)?.gameObject) != self);
            }
            return accValues
                .Select(each =>
                {
                    // ReSharper disable once InvertIf
                    if (each.ResourceType == PathResolver.ResourceType.File)
                    {
                        string assetPath = string.IsNullOrEmpty(each.FolderPath)
                            ? (string)each.Resource
                            : $"{each.FolderPath}/{each.Resource}";
                        // Debug.Log(assetPath);
                        try
                        {
                            return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        }
#pragma warning disable CS0168
                        catch (Exception e)
#pragma warning restore CS0168
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogException(e);
#endif
                            return null;
                        }
                    }

                    return each.Resource;
                })
                .Where(each => !RuntimeUtil.IsNull(each))
                .Select(each => ValidateXPathResult(property.serializedObject.targetObject, each, expectedType, expectedInterface))
                .Where(each => each.valid)
                .Select(each => each.value);

        }

        public static IReadOnlyList<PathResolver.Node> ResolvePath(string path, SerializedProperty property,
            MemberInfo member, object target, bool hierarchyNames = false)
        {
            return PathResolver.Resolve(PathParser.Parse(path, !hierarchyNames),
                PathResolver.CreateContext(property, member, target), hierarchyNames);
        }

        private static IEnumerable<XPathStep> PrepareShortcutSteps(IEnumerable<XPathStep> steps, Type expectedType, Type expectedInterface)
        {
            foreach (XPathStep original in steps)
            {
                XPathStep step = original;
                if (step.Attr is XPathAttrGetComponents components)
                {
                    string typeName = components.TypeName;
                    if (!string.IsNullOrEmpty(typeName))
                    {
                        if (PathResolver.FindTypes(typeName).Contains(typeof(GameObject)))
                        {
                            step.Attr = null;
                            step.CollectionOperations = Array.Empty<XPathCollectionOperation>();
                        }
                    }
                    else if (typeof(Component).IsAssignableFrom(expectedType))
                    {
                        step.Attr = new XPathAttrGetComponents(expectedType.FullName, components.FirstOnly);
                    }
                    else
                    {
                        step.Attr = null;
                        step.CollectionOperations = Array.Empty<XPathCollectionOperation>();
                    }
                    if (step.Attr != null && expectedInterface != null)
                    {
                        step.Predicates = (step.Predicates ?? Array.Empty<IReadOnlyList<XPathPredicate>>()).Concat(new[]
                        {
                            (IReadOnlyList<XPathPredicate>)new[] { new XPathPredicate
                            {
                                Attr = new XPathAttrAs(expectedInterface.FullName),
                                FilterComparer = new FilterComparerTruly(),
                            } },
                        }).ToArray();
                    }
                }
                yield return step;
            }
        }

        public static IReadOnlyList<Type> FindAllPossibleTypes(string shortOrQualifiedName)
        {
            return PathResolver.FindTypes(shortOrQualifiedName);
        }
    }
}
