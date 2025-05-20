#if WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE && !SAINTSFIELD_WWISE_DISABLE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using SaintsField.Utils;
using SaintsField.Wwise;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Wwise.GetBankDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GetBankAttribute), true)]
    public partial class GetBankAttributeDrawer: GetByXPathAttributeDrawer
    {
        private const string PropNameWwiseObjectReference = "WwiseObjectReference";

        protected override void ActualSignPropertyCache(PropertyCache propertyCache)
        {
            HelperDoSignPropertyCache(propertyCache);
        }

        private static void HelperDoSignPropertyCache(PropertyCache propertyCache)
        {
            propertyCache.SerializedProperty.FindPropertyRelative(PropNameWwiseObjectReference).objectReferenceValue = (UnityEngine.Object)propertyCache.TargetValue;
        }

        protected override (string error, object value) GetCurValue(SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            return ("", property.FindPropertyRelative(PropNameWwiseObjectReference).objectReferenceValue);
        }

        protected override GetXPathValuesResult GetXPathValues(IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface,
            SerializedProperty property, MemberInfo info, object parent)
        {
            return CalcXPathValues(andXPathInfoList, expectedType, expectedInterface, property, info, parent);
        }

        private readonly struct Processing
        {
            public readonly AkWwiseProjectData.AkInformation Target;
            public readonly List<string> XPathGroupSegs;

            public Processing(AkWwiseProjectData.AkInformation target, List<string> xPathGroupSegs)
            {
                Target = target;
                XPathGroupSegs = xPathGroupSegs;
            }
        }

        private static readonly Dictionary<Guid, string> GuidToPath = new Dictionary<Guid, string>();

        protected override SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo MakeObjectBaseInfo(UnityEngine.Object objResult,
            string assetPath)
        {
            if (objResult is WwiseObjectReference wwiseObjectReference)
            {
                return new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                    wwiseObjectReference,
                    wwiseObjectReference.ObjectName,
                    "Wwise Bank",
                    GuidToPath.GetValueOrDefault(wwiseObjectReference.Guid, "")
                );
            }

            if (!objResult)
            {
                return SaintsObjectPickerWindowUIToolkit.NoneObjectInfo;
            }

            throw new ArgumentException($"Unsupported args {objResult}", nameof(objResult));
        }

        private static (bool hasResults, IEnumerable<WwiseObjectReference> results) GetMatchedWwiseObject(IReadOnlyList<AkWwiseProjectData.AkInfoWorkUnit> banks, IReadOnlyList<XPathStep> xPathSteps)
        {
            List<Processing> allInfoFlatten = banks
                .SelectMany(bank => bank.List)
                .Select(each => new Processing(
                    each,
                    new List<string>(each.Path.Replace('\\', '/').Split('/'))
                ))
                .ToList();

            foreach (Processing processing in allInfoFlatten)
            {
                GuidToPath[processing.Target.Guid] = processing.Target.Path;
            }

            IEnumerable<Processing> allInfos = allInfoFlatten;

            // return (false, Array.Empty<WwiseObjectReference>());

            foreach (XPathStep xPathStep in xPathSteps)
            {
                // Debug.Log(xPathStep.SepCount);
                allInfos = xPathStep.SepCount == 2
                    ? ExpandSeps(allInfos, xPathStep.NodeTest)
                    : FilterOutNodeTest(allInfos, xPathStep.NodeTest);

                XPathPredicate indexPredicte = xPathStep.Predicates
                    .SelectMany(each => each)
                    // ReSharper disable once MergeIntoPattern
                    .FirstOrDefault(each => each.Attr is XPathAttrIndex && each.FilterComparer is FilterComparerInt);

                if (indexPredicte.FilterComparer is FilterComparerInt compare)
                {
                    // FilterComparerInt compare = (FilterComparerInt)indexPredicte.FilterComparer;
                    int indexValue = compare.Value;
                    IReadOnlyList<Processing> allInfoExpanded = allInfos.ToArray();
                    int useIndex = indexValue % allInfoExpanded.Count;
                    if (useIndex < 0)
                    {
                        useIndex += allInfoExpanded.Count;
                    }

                    allInfos = new[]{allInfoExpanded[useIndex]};
                }
            }

            (bool hasElement, IEnumerable<Processing> elements) = Util.HasAnyElement(allInfos.DistinctBy(each => each.Target.Guid));
            return (hasElement, elements.Select(each => WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType.Soundbank, each.Target.Name,
                each.Target.Guid)));
        }

        private static IEnumerable<Processing> FilterOutNodeTest(IEnumerable<Processing> allInfos, NodeTest nodeTest)
        {
            foreach (Processing processing in allInfos)
            {
                if (processing.XPathGroupSegs.Count == 0)
                {
                    continue;
                }

                if (NodeTestMatch.NodeMatch(processing.XPathGroupSegs[0], nodeTest))
                {
                    processing.XPathGroupSegs.RemoveAt(0);
                    if(processing.XPathGroupSegs.Count > 0)
                    {

                        yield return processing;
                    }
                }
            }
        }

        private static IEnumerable<Processing> ExpandSeps(IEnumerable<Processing> allInfos, NodeTest nodeTest)
        {

            return allInfos
                .SelectMany(each =>
                    ExpandSepLis(each.XPathGroupSegs, nodeTest)
                        .Select(lis => new Processing(each.Target, lis))
                );
            // return allInfos[0]
        }

        private static IEnumerable<List<string>> ExpandSepLis(List<string> eachXPathGroupSegs, NodeTest nodeTest)
        {
            // Debug.Log($"TEST {nodeTest}: {string.Join("/", eachXPathGroupSegs)}");
            // yield break;
            for (int startIndex = 0; startIndex < eachXPathGroupSegs.Count; startIndex++)
            {
                string first = eachXPathGroupSegs[startIndex];
                // Debug.Log($"FIRST {first}");
                if(NodeTestMatch.NodeMatch(first, nodeTest))
                {
                    yield return eachXPathGroupSegs.Skip(startIndex).ToList();
                    // Debug.Log($"MATCHED  {nodeTest}: {string.Join("/", result)}");

                }

            }
        }

        private static GetXPathValuesResult CalcXPathValues(IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, MemberInfo info, object parent)
        {
            AkWwiseProjectData wwiseData = AkWwiseProjectInfo.GetData();
            List<AkWwiseProjectData.AkInfoWorkUnit> banks = wwiseData.BankWwu;

            bool anyResult = false;
            List<string> errors = new List<string>();
            // IEnumerable<object> finalResults = Array.Empty<object>();
            List<IEnumerable<WwiseObjectReference>> finalResultsCollected = new List<IEnumerable<WwiseObjectReference>>();

            foreach (XPathResourceInfo orXPathInfoList in andXPathInfoList)
            {
                foreach (GetByXPathAttribute.XPathInfo xPathInfo in orXPathInfoList.OrXPathInfoList)
                {
                    IReadOnlyList<XPathStep> xPathSteps;
                    if (xPathInfo.IsCallback)
                    {
                        (string error, string xPathString) = Util.GetOf(xPathInfo.Callback, "", property, info, parent);

                        if (error != "")
                        {
                            errors.Add(error);
                            continue;
                        }

                        xPathSteps = XPathParser.Parse(xPathString).ToArray();
                    }
                    else
                    {
                        xPathSteps = xPathInfo.XPathSteps;
                    }

                    (bool hasResults, IEnumerable<WwiseObjectReference> results) = GetMatchedWwiseObject(banks, xPathSteps);

                    // ReSharper disable once InvertIf
                    if (hasResults)
                    {
                        anyResult = true;
                        finalResultsCollected.Add(results);
                    }
                }
            }

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

        public static bool HelperGetArraySize(SerializedProperty arrayProperty, FieldInfo info, bool isImGui)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }

            if (arrayProperty.arraySize > 0)
            {
                return isImGui;
            }

            string key = arrayProperty.propertyPath;

            GetByXPathGenericCache target = new GetByXPathGenericCache
            {
                // ImGuiRenderCount = 1,
                Error = "",
                // GetByXPathAttributes = attributes,
                ArrayProperty = arrayProperty,
            };

            if (SharedCache.TryGetValue(key, out GetByXPathGenericCache exists))
            {
                target = exists;
            }

            (GetByXPathAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<GetByXPathAttribute>(arrayProperty);
            target.GetByXPathAttributes = attributes;

            if(NothingSigner(target.GetByXPathAttributes[0]))
            {
                return false;
            }

            (string typeError, Type expectType, Type expectInterface) = GetExpectedTypeOfProp(arrayProperty, info);

            // Debug.Log($"array expectType={expectType}");

            if (typeError != "")
            {
                return false;
            }

            target.ExpectedType = expectType;
            target.ExpectedInterface = expectInterface;

            IReadOnlyList<object> expandedResults;
            // if(true)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# refresh resources for {arrayProperty.propertyPath}");
#endif

                GetXPathValuesResult iterResults = CalcXPathValues(
                    target.GetByXPathAttributes
                        .Select(xPathAttribute => new XPathResourceInfo
                        {
                            OptimizationPayload = xPathAttribute.OptimizationPayload,
                            OrXPathInfoList = xPathAttribute.XPathInfoAndList.SelectMany(each => each).ToArray(),
                        })
                        .ToArray(),
                    target.ExpectedType,
                    target.ExpectedInterface,
                    arrayProperty,
                    info,
                    parent);

                expandedResults = iterResults.Results.ToArray();
                target.CachedResults = expandedResults;
            }

            if (expandedResults.Count == 0)
            {
                return true;
            }

            arrayProperty.arraySize = expandedResults.Count;
            EnqueueSceneViewNotification($"Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
            Debug.Log($"#GetByXPath# Helper: Adjust array {arrayProperty.displayName} to length {arrayProperty.arraySize}");
#endif
            arrayProperty.serializedObject.ApplyModifiedProperties();

            GetByXPathAttribute getByXPathAttribute = target.GetByXPathAttributes[0];

            if(getByXPathAttribute.InitSign)
            {
                foreach ((object targetResult, int propertyCacheKey) in expandedResults.WithIndex())
                {
                    SerializedProperty processingProperty;
                    try
                    {
                        processingProperty =
                            target.ArrayProperty.GetArrayElementAtIndex(propertyCacheKey);
                    }
#pragma warning disable CS0168
                    catch (NullReferenceException e)
#pragma warning restore CS0168
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogException(e);
#endif

                        return false;
                    }

                    (SerializedUtils.FieldOrProp fieldOrProp, object fieldParent) =
                        SerializedUtils.GetFieldInfoAndDirectParent(processingProperty);
                    PropertyCache propertyCache
                        = target.IndexToPropertyCache[propertyCacheKey]
                            = new PropertyCache
                            {
                                // ReSharper disable once RedundantCast
                                MemberInfo = fieldOrProp.IsField ? (MemberInfo)fieldOrProp.FieldInfo : fieldOrProp.PropertyInfo,
                                Parent = fieldParent,
                                SerializedProperty = processingProperty,
                            };

                    propertyCache.OriginalValue = null;
                    propertyCache.TargetValue = targetResult;
                    bool targetIsNull = RuntimeUtil.IsNull(targetResult);
                    propertyCache.TargetIsNull = targetIsNull;

                    propertyCache.MisMatch = !targetIsNull;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                    Debug.Log($"#GetByXPath# Helper: Sign {propertyCache.SerializedProperty.propertyPath} from {propertyCache.OriginalValue} to {propertyCache.TargetValue}");
#endif

                    bool canSign = HelperPreDoSignPropertyCache(propertyCache, true);

                    // ReSharper disable once InvertIf
                    if (canSign)
                    {
                        HelperDoSignPropertyCache(propertyCache);
                        HelperPostDoSignPropertyCache(propertyCache);
                        propertyCache.SerializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_GET_BY_XPATH
                Debug.Log($"#GetByXPath# Helper: Apply changes to {arrayProperty.serializedObject.targetObject}");
#endif
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }


            SharedCache[key] = target;

            return isImGui;
        }
    }
}

#endif
