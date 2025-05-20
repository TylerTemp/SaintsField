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
using SaintsField.Wwise;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.Wwise.WwiseAutoGetterDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GetWwiseAttribute), true)]
    public partial class WwiseAutoGetterAttributeDrawer: GetByXPathAttributeDrawer
    {
        private const string PropNameWwiseObjectReference = "WwiseObjectReference";

        protected override void ActualSignPropertyCache(PropertyCache propertyCache)
        {
            HelperDoSignPropertyCache(propertyCache);
        }

        public static void HelperDoSignPropertyCache(PropertyCache propertyCache)
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
            return CalcXPathValues(WwiseAutoGetterAttributeDrawerHelper.GetWwiseObjectType(expectedType), andXPathInfoList, expectedType, expectedInterface, property, info, parent);
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

        private static readonly Dictionary<Guid, (string path, WwiseObjectType wwiseType)> GuidToPath = new Dictionary<Guid, (string path, WwiseObjectType wwiseType)>();

        protected override SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo MakeObjectBaseInfo(UnityEngine.Object objResult,
            string assetPath)
        {
            if (objResult is WwiseObjectReference wwiseObjectReference)
            {
                string path = "";
                string type = "";
                // ReSharper disable once InvertIf
                if (GuidToPath.TryGetValue(wwiseObjectReference.Guid, out (string path, WwiseObjectType wwiseType) value))
                {
                    path = value.path;
                    type = value.wwiseType.ToString();
                }
                return new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                    wwiseObjectReference,
                    wwiseObjectReference.ObjectName,
                    type,
                    path
                );
            }

            if (!objResult)
            {
                return SaintsObjectPickerWindowUIToolkit.NoneObjectInfo;
            }

            throw new ArgumentException($"Unsupported args {objResult}", nameof(objResult));
        }

        private static (bool hasResults, IEnumerable<WwiseObjectReference> results) GetMatchedWwiseObject(WwiseObjectType wwiseObjectType, IReadOnlyList<AkWwiseProjectData.AkInformation> akInfos, IReadOnlyList<XPathStep> xPathSteps)
        {
            // List<Processing> allInfoFlatten =
            //     .ToList();

            // foreach (Processing processing in allInfoFlatten)
            // {
            //     GuidToPath[processing.Target.Guid] = processing.Target.Path;
            // }

            IEnumerable<Processing> allInfos = akInfos
                .Select(each => new Processing(
                    each,
                    new List<string>(each.Path.Replace('\\', '/').Split('/').Skip(1))
                ));

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

            (bool hasElement, IEnumerable<Processing> elements) = Util.HasAnyElement(allInfos
                .Where(each => each.XPathGroupSegs.Count == 0)
                .DistinctBy(each => each.Target.Guid));

            return (hasElement, elements.Select(each =>
            {
                GuidToPath[each.Target.Guid] = (each.Target.Path, wwiseObjectType);
                return WwiseObjectReference.FindOrCreateWwiseObject(wwiseObjectType, each.Target.Name,
                    each.Target.Guid);
            }));
        }

        private static IEnumerable<Processing> FilterOutNodeTest(IEnumerable<Processing> allInfos, NodeTest nodeTest)
        {
            foreach (Processing processing in allInfos)
            {
                if (processing.XPathGroupSegs.Count == 0)
                {
                    continue;
                }

                // Debug.Log($"{processing.XPathGroupSegs[0]} MATCH {nodeTest} = {NodeTestMatch.NodeMatch(processing.XPathGroupSegs[0], nodeTest)}");

                if (NodeTestMatch.NodeMatch(processing.XPathGroupSegs[0], nodeTest))
                {
                    // Debug.Log($"{processing.XPathGroupSegs[0]} MATCH {nodeTest}; curFull={string.Join("/", processing.XPathGroupSegs)}");
                    processing.XPathGroupSegs.RemoveAt(0);
                    // if(processing.XPathGroupSegs.Count > 0)
                    // {
                    //     yield return processing;
                    // }
                    yield return processing;
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
            // Debug.Log($"TEST: {string.Join("/", eachXPathGroupSegs)}");
            // yield break;
            for (int startIndex = 0; startIndex < eachXPathGroupSegs.Count; startIndex++)
            {
                string first = eachXPathGroupSegs[startIndex];
                // Debug.Log($"FIRST {first} MATCH = {NodeTestMatch.NodeMatch(first, nodeTest)}");
                if(NodeTestMatch.NodeMatch(first, nodeTest))
                {
                    yield return eachXPathGroupSegs.Skip(startIndex + 1).ToList();
                    // Debug.Log($"MATCHED  {nodeTest}: {string.Join("/", r)}");
                    // yield return r;
                }
            }
        }

        public static GetXPathValuesResult CalcXPathValues(WwiseObjectType wwiseObjectType, IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, MemberInfo info, object parent)
        {
            AkWwiseProjectData wwiseData = AkWwiseProjectInfo.GetData();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            IEnumerable<AkWwiseProjectData.AkInformation> wwiseInfoIe = wwiseObjectType switch
            {
                WwiseObjectType.AuxBus => wwiseData.AuxBusWwu.SelectMany(each => each.List),
                WwiseObjectType.Event => wwiseData.EventWwu.SelectMany(each => each.List),
                WwiseObjectType.Soundbank => wwiseData.BankWwu.SelectMany(each => each.List),
                WwiseObjectType.State => wwiseData.StateWwu.SelectMany(each => each.List),
                WwiseObjectType.Switch => wwiseData.SwitchWwu.SelectMany(each => each.List),
                WwiseObjectType.GameParameter => wwiseData.RtpcWwu.SelectMany(each => each.List),
                WwiseObjectType.Trigger => wwiseData.TriggerWwu.SelectMany(each => each.List),
                WwiseObjectType.AcousticTexture => wwiseData.AcousticTextureWwu.SelectMany(each => each.List),
                WwiseObjectType.None => Array.Empty<AkWwiseProjectData.AkInformation>(),
                _ => throw new ArgumentOutOfRangeException(nameof(wwiseObjectType), wwiseObjectType, null),
            };

            bool anyResult = false;
            List<string> errors = new List<string>();
            // IEnumerable<object> finalResults = Array.Empty<object>();
            List<IEnumerable<WwiseObjectReference>> finalResultsCollected = new List<IEnumerable<WwiseObjectReference>>();

            IReadOnlyList<AkWwiseProjectData.AkInformation> wwiseInfos = wwiseInfoIe.ToArray();

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

                    (bool hasResults, IEnumerable<WwiseObjectReference> results) = GetMatchedWwiseObject(wwiseObjectType, wwiseInfos, xPathSteps);

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
    }
}

#endif
