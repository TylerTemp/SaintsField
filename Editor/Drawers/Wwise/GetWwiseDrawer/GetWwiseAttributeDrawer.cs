#if (WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || WWISE_2021_OR_LATER || WWISE_2020_OR_LATER || WWISE_2019_OR_LATER || WWISE_2018_OR_LATER || WWISE_2017_OR_LATER || WWISE_2016_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE
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
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Wwise.GetWwiseDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(GetWwiseAttribute), true)]
    public partial class GetWwiseAttributeDrawer: GetByXPathAttributeDrawer
    {
        private const string PropNameWwiseObjectReference = "WwiseObjectReference";

        protected override void ActualSignPropertyCache(PropertyCache propertyCache)
        {
            HelperDoSignPropertyCache(propertyCache);
        }

        public static void HelperDoSignPropertyCache(PropertyCache propertyCache)
        {
            // Debug.Log($"Do Sign {propertyCache}");
            UnityEngine.Object targetValue = (UnityEngine.Object)propertyCache.TargetValue;
            SerializedProperty property = propertyCache.SerializedProperty;
            MemberInfo info = propertyCache.MemberInfo;
            Type infoRawType;

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    infoRawType = ((FieldInfo)info).FieldType;
                    break;
                case MemberTypes.Property:
                    infoRawType = ((PropertyInfo)info).PropertyType;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info), info, null);
            }

            SerializedProperty prop = propertyCache.SerializedProperty.FindPropertyRelative(PropNameWwiseObjectReference);
            if (prop == null)
            {
                Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) == -1
                    ? infoRawType
                    : ReflectUtils.GetElementType(infoRawType);

                string wrapPropName = ReflectUtils.GetIWrapPropName(rawType);
                SerializedProperty wrapProp = property.FindPropertyRelative(wrapPropName) ??
                                              SerializedUtils.FindPropertyByAutoPropertyName(property, wrapPropName);

                prop = wrapProp.FindPropertyRelative(PropNameWwiseObjectReference) ??
                       SerializedUtils.FindPropertyByAutoPropertyName(wrapProp,
                           PropNameWwiseObjectReference);

            }

            // Debug.Log($"sign {prop.propertyPath} to {targetValue}");

            prop.objectReferenceValue = targetValue;
        }

        protected override (string error, object value) GetCurValue(SerializedProperty property, MemberInfo memberInfo, object parent)
        {
            SerializedProperty prop = property.FindPropertyRelative(PropNameWwiseObjectReference);
            if(prop != null)
            {
                return ("", prop.objectReferenceValue);
            }

            (string error, int _, object value) = Util.GetValue(property, memberInfo, parent);
            if (error != "")
            {
                return (error, value);
            }

            // ReSharper disable once InvertIf
            if (value is IWrapProp wrapProp)
            {
                object w = Util.GetWrapValue(wrapProp);
                if (w is AK.Wwise.BaseType bt)
                {
                    return ("", bt.ObjectReference);
                }
            }

            return ($"Unsupported type {value?.GetType()}", null);
        }

        protected override GetXPathValuesResult GetXPathValues(IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface,
            SerializedProperty property, MemberInfo info, object parent)
        {
            return CalcXPathValues(GetWwiseAttributeDrawerHelper.GetWwiseObjectType(expectedType), andXPathInfoList, expectedType, expectedInterface, property, info, parent);
        }

        private readonly struct Processing
        {
            public readonly WwiseBasicInfo Target;
            public readonly List<string> XPathGroupSegs;

            public Processing(WwiseBasicInfo target, List<string> xPathGroupSegs)
            {
                Target = target;
                XPathGroupSegs = xPathGroupSegs;
            }
        }

        private static readonly Dictionary<Guid, WwiseBasicInfo> GuidToPath = new Dictionary<Guid, WwiseBasicInfo>();

        protected override SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo MakeObjectBaseInfo(UnityEngine.Object objResult,
            string assetPath)
        {
            if (objResult is WwiseObjectReference wwiseObjectReference)
            {
                string path = "";
                string type = "";
                // ReSharper disable once InvertIf
                if (GuidToPath.TryGetValue(wwiseObjectReference.Guid, out WwiseBasicInfo value))
                {
                    path = string.Join("/", value.BasicPathSegments);
                    type = value.WwiseObjectType.ToString();
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

        private static (bool hasResults, IEnumerable<WwiseObjectReference> results) GetMatchedWwiseObject(WwiseObjectType wwiseObjectType, IReadOnlyList<WwiseBasicInfo> akInfos, IReadOnlyList<XPathStep> xPathSteps)
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
                    new List<string>(each.BasicPathSegments)
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
                GuidToPath[each.Target.Guid] = each.Target;
                return WwiseObjectReference.FindOrCreateWwiseObject(wwiseObjectType, each.Target.Name,
                    each.Target.Guid);
            }));
        }

        private readonly struct WwiseBasicInfo
        {
            public readonly WwiseObjectType WwiseObjectType;
            public readonly Guid Guid;
            public readonly string Name;
            public readonly IReadOnlyList<string> BasicPathSegments;

            public WwiseBasicInfo(WwiseObjectType wwiseObjectType, Guid guid, string name, IReadOnlyList<string> basicPathSegments)
            {
                WwiseObjectType = wwiseObjectType;
                Guid = guid;
                Name = name;
                BasicPathSegments = basicPathSegments;
            }
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

        private static IReadOnlyList<string> ParseBasicPath(string rawPath) => rawPath.Replace("\\", "/").Split('/');

        private static IEnumerable<WwiseBasicInfo> ParseToBasicInfo(WwiseObjectType wwiseObjectType, AkWwiseProjectData.AkInfoWorkUnit workUnit)
        {
            return workUnit.List.Select(akInformation => new WwiseBasicInfo(wwiseObjectType, akInformation.Guid, akInformation.Name,
                ParseBasicPath(akInformation.Path).Skip(1).ToArray()));
        }

        private static IEnumerable<WwiseBasicInfo> ParseToBasicInfo(WwiseObjectType wwiseObjectType, AkWwiseProjectData.EventWorkUnit workUnit)
        {
            return workUnit.List.Select(akEvent => new WwiseBasicInfo(wwiseObjectType, akEvent.Guid, akEvent.Name,
                ParseBasicPath(akEvent.Path).Skip(1).ToArray()));
        }

        private static IEnumerable<WwiseBasicInfo> ParseToBasicInfo(WwiseObjectType wwiseObjectType, AkWwiseProjectData.GroupValWorkUnit workUnit)
        {
            foreach (AkWwiseProjectData.GroupValue groupValue in workUnit.List)
            {
                // Debug.Log($"groupValue.Path={groupValue.Path}");
                string[] basePath = ParseBasicPath(groupValue.Path).Skip(1).ToArray();
                foreach (AkWwiseProjectData.AkBaseInformation baseInfo in groupValue.values)
                {
                    // Debug.Log(baseInfo.Name);
                    // Debug.Log(baseInfo.Id);
                    List<string> newPath = new List<string>(basePath)
                    {
                        baseInfo.Name,
                    };
                    yield return new WwiseBasicInfo(wwiseObjectType, baseInfo.Guid, baseInfo.Name, newPath);
                }
            }
        }

        public static GetXPathValuesResult CalcXPathValues(WwiseObjectType wwiseObjectType, IReadOnlyList<XPathResourceInfo> andXPathInfoList, Type expectedType, Type expectedInterface, SerializedProperty property, MemberInfo info, object parent)
        {
            AkWwiseProjectData wwiseData = AkWwiseProjectInfo.GetData();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            IReadOnlyList<WwiseBasicInfo> wwiseInfos = wwiseObjectType switch
            {
                WwiseObjectType.AuxBus => wwiseData.AuxBusWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.Event => wwiseData.EventWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.Soundbank => wwiseData.BankWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.State => wwiseData.StateWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.Switch => wwiseData.SwitchWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.GameParameter => wwiseData.RtpcWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.Trigger => wwiseData.TriggerWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.AcousticTexture => wwiseData.AcousticTextureWwu.SelectMany(each => ParseToBasicInfo(wwiseObjectType, each)).ToArray(),
                WwiseObjectType.None => Array.Empty<WwiseBasicInfo>(),
                _ => throw new ArgumentOutOfRangeException(nameof(wwiseObjectType), wwiseObjectType, null),
            };

            // foreach (WwiseBasicInfo wwiseBasicInfo in wwiseInfos)
            // {
            //     Debug.Log(string.Join("/", wwiseBasicInfo.BasicPathSegments));
            // }

            bool anyResult = false;
            List<string> errors = new List<string>();
            // IEnumerable<object> finalResults = Array.Empty<object>();
            List<IEnumerable<WwiseObjectReference>> finalResultsCollected = new List<IEnumerable<WwiseObjectReference>>();

            // Debug.Log(wwiseInfos.Count);

            foreach (XPathResourceInfo orXPathInfoList in andXPathInfoList)
            {
                foreach (GetByXPathAttribute.XPathInfo xPathInfo in orXPathInfoList.OrXPathInfoList)
                {
                    IReadOnlyList<XPathStep> xPathSteps;
                    // Debug.Log($"xPathInfo.IsCallback={xPathInfo.IsCallback}/{xPathInfo.Callback}");
                    if (xPathInfo.IsCallback)
                    {
                        (string error, string xPathString) = Util.GetOf(xPathInfo.Callback, "", property, info, parent);

                        if (error != "")
                        {
                            errors.Add(error);
                            continue;
                        }

                        string actualString = xPathString.StartsWith('/') ? xPathString : $"//{xPathString}";

                        xPathSteps = XPathParser.Parse(actualString).ToArray();
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
