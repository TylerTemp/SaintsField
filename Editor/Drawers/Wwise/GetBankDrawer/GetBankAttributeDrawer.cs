using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.XPathDrawers.GetByXPathDrawer;
using SaintsField.Editor.Utils;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Wwise.GetBankDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GetBankAttributeDrawer))]
    public partial class GetBankAttributeDrawer: GetByXPathAttributeDrawer
    {
        private const string PropNameWwiseObjectReference = "WwiseObjectReference";

        protected override void ActualSignPropertyCache(PropertyCache propertyCache)
        {
            propertyCache.SerializedProperty.objectReferenceValue = (UnityEngine.Object)propertyCache.TargetValue;
        }

        protected override (string error, object value) GetCurValue(SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            return ("", property.FindPropertyRelative(PropNameWwiseObjectReference).objectReferenceValue);
        }

        protected override GetXPathValuesResult GetXPathValues(IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface,
            SerializedProperty property, MemberInfo info, object parent)
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

        private static (bool hasResults, IEnumerable<WwiseObjectReference> results) GetMatchedWwiseObject(IReadOnlyList<AkWwiseProjectData.AkInfoWorkUnit> banks, IReadOnlyList<XPathStep> xPathSteps)
        {
            List<Processing> allInfos = banks
                .SelectMany(bank => bank.List)
                .Select(each => new Processing(
                    each,
                    new List<string>(each.Path.Split('/'))
                ))
                .ToList();

            foreach (XPathStep xPathStep in xPathSteps)
            {
                allInfos = xPathStep.SepCount == 2
                    ? ExpandSeps(allInfos)
                    : allInfos;

                allInfos = FilterOutNodeTest(allInfos, xPathStep.NodeTest).ToList();
                XPathPredicate indexPredicte = xPathStep.Predicates
                    .SelectMany(each => each)
                    // ReSharper disable once MergeIntoPattern
                    .FirstOrDefault(each => each.Attr is XPathAttrIndex && each.FilterComparer is FilterComparerInt);

                if (indexPredicte.FilterComparer is FilterComparerInt compare)
                {
                    // FilterComparerInt compare = (FilterComparerInt)indexPredicte.FilterComparer;
                    int indexValue = compare.Value;
                    int useIndex = indexValue % allInfos.Count;
                    if (useIndex < 0)
                    {
                        useIndex += allInfos.Count;
                    }

                    allInfos = new List<Processing>
                    {
                        allInfos[useIndex],
                    };
                }
            }

            return (allInfos.Count > 0, allInfos.Select(each =>
                WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType.Soundbank, each.Target.Name,
                    each.Target.Guid)));
        }

        private static IEnumerable<Processing> FilterOutNodeTest(List<Processing> allInfos, NodeTest nodeTest)
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
                    yield return processing;
                }
            }
        }

        private static List<Processing> ExpandSeps(List<Processing> allInfos)
        {
            return allInfos
                .SelectMany(each =>
                    ExpandSepLis(each.XPathGroupSegs)
                        .Select(lis => new Processing(each.Target, lis))
                )
                .ToList();
        }

        private static IEnumerable<List<string>> ExpandSepLis(List<string> eachXPathGroupSegs)
        {
            for (int startIndex = 0; startIndex < eachXPathGroupSegs.Count; )
            {
                yield return eachXPathGroupSegs.Skip(startIndex).ToList();
            }
        }
    }
}
