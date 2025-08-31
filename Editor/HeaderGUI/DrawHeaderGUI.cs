using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.ComponentHeader;
using SaintsField.Editor.Core;
using SaintsField.Editor.HeaderGUI.Drawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.HeaderGUI
{
    public class DrawHeaderGUI
    {
#if SAINTSFIELD_HEADER_GUI
        [InitializeOnLoadMethod]
#endif
        public static void DelayInit()
        {
            EditorApplication.delayCall += EnsureInitLoad;
            EditorApplication.delayCall += LoadTypeToRenderTargetInfo;
            EditorApplication.delayCall += ManuallyUpdate;
        }

        private static void ManuallyUpdate()
        {
            HelperUpdate();
            EditorApplication.delayCall += ManuallyUpdate;
        }

        private static FieldInfo _sEditorHeaderItemsMethods;

        private static bool _initLoad;

        public static void EnsureInitLoad()
        {
            if (_initLoad)
            {
                return;
            }

            InitLoad();
        }

        private static void InitLoad()
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            if(_sEditorHeaderItemsMethods == null)
            {
                _sEditorHeaderItemsMethods = typeof(EditorGUIUtility).GetField("s_EditorHeaderItemsMethods", flags);
            }
            if (_sEditorHeaderItemsMethods == null)
            {
                return;  // API is changed internally, and it's now gone
            }

            IList value = (IList)_sEditorHeaderItemsMethods.GetValue(null);
            // Debug.Log($"value={value}");
            if (value == null)
            {
                EditorApplication.delayCall += InitLoad;
                return;
            }

            Type delegateType = value.GetType().GetGenericArguments()[0];

            // TypeCache.GetMethodsWithAttribute<AbsComponentHeaderAttribute>();

            MethodInfo methodInfo = typeof(DrawHeaderGUI).GetMethod(nameof(DrawMethod), flags);

            // Debug.Log($"inject {methodInfo} into {value}");

            // ReSharper disable once AssignNullToNotNullAttribute
            value.Add(Delegate.CreateDelegate(delegateType, methodInfo));
            _initLoad = true;
        }

        public enum MemberType
        {
            Method,
            Field,
            Property,
            Class,
        }

        public readonly struct RenderTargetInfo : IEquatable<RenderTargetInfo>
        {
            public readonly AbsComponentHeaderAttribute Attribute;
            public readonly MemberType MemberType;
            public readonly MemberInfo MemberInfo;
            public readonly int SortOrder;

            public RenderTargetInfo(AbsComponentHeaderAttribute attribute, MemberType memberType, MemberInfo memberInfo, int sortOrder)
            {
                Attribute = attribute;
                MemberType = memberType;
                MemberInfo = memberInfo;
                SortOrder = sortOrder;
            }

            public override string ToString() => $"<RenderTargetInfo {MemberInfo.Name} {Attribute.GetType()} {Attribute.IsLeft} {Attribute.GroupBy} />";

            public bool Equals(RenderTargetInfo other)
            {
                return Equals(Attribute, other.Attribute) && MemberType == other.MemberType && Equals(MemberInfo, other.MemberInfo);
            }

            public override bool Equals(object obj)
            {
                return obj is RenderTargetInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(Attribute, (int)MemberType, MemberInfo);
                // return HashCode.Combine(Attribute, (int)MemberType, MemberInfo);
            }
        }

        // private static readonly List<RenderTargetInfo> RenderTargetInfos = new List<RenderTargetInfo>();
        private static readonly Dictionary<Type, List<RenderTargetInfo>> CachedTypeToRenderTargetInfos = new Dictionary<Type, List<RenderTargetInfo>>();

        // ReSharper disable once MemberCanBePrivate.Global
        public static void LoadTypeToRenderTargetInfo()
        {
            foreach (MethodInfo methodInfo in TypeCache.GetMethodsWithAttribute<AbsComponentHeaderAttribute>())
            {
                Type reflectedType = methodInfo.ReflectedType;
                if (reflectedType == null)
                {
                    continue;
                }

                // Debug.Log(containerType);

                foreach (AbsComponentHeaderAttribute attribute in ReflectCache.GetCustomAttributes<AbsComponentHeaderAttribute>(methodInfo))
                {
                    switch (attribute)
                    {
                        case HeaderButtonAttribute _:
                        {
                            if (!HeaderButtonDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                            {
                                continue;
                            }
                        }
                            break;
                        case HeaderDrawAttribute _:
                        {
                            if(!HeaderDrawDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                            {
                                continue;
                            }
                        }
                            break;
                        case HeaderLabelAttribute _:
                        {
                            if (!HeaderLabelDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                            {
                                continue;
                            }
                        }
                            break;
                    }

                    RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Method, methodInfo, methodInfo.MetadataToken);

                    // subclass
                    foreach (KeyValuePair<Type, List<RenderTargetInfo>> cachedKv in CachedTypeToRenderTargetInfos)
                    {
                        if (reflectedType.IsAssignableFrom(cachedKv.Key) && reflectedType != cachedKv.Key)
                        {
                            cachedKv.Value.Add(info);
                        }
                    }

                    if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType, out List<RenderTargetInfo> renderTargetInfos))
                    {
                        CachedTypeToRenderTargetInfos[reflectedType] = renderTargetInfos = new List<RenderTargetInfo>();
                    }

                    if (!renderTargetInfos.Contains(info))
                    {
                        // Debug.Log(methodInfo.Name);
                        renderTargetInfos.Add(info);
                    }
                }

            }

#if UNITY_2021_3_OR_NEWER
            foreach (FieldInfo fieldInfo in TypeCache.GetFieldsWithAttribute<AbsComponentHeaderAttribute>())
            {
                Type reflectedType = fieldInfo.ReflectedType;
                if (reflectedType == null)
                {
                    continue;
                }

                // Debug.Log(containerType);

                foreach (AbsComponentHeaderAttribute attribute in ReflectCache.GetCustomAttributes<AbsComponentHeaderAttribute>(fieldInfo))
                {
                    RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Field, fieldInfo, fieldInfo.MetadataToken);

                    // subclass
                    foreach (KeyValuePair<Type, List<RenderTargetInfo>> cachedKv in CachedTypeToRenderTargetInfos)
                    {
                        if (reflectedType.IsAssignableFrom(cachedKv.Key) && reflectedType != cachedKv.Key)
                        {
                            cachedKv.Value.Add(info);
                        }
                    }

                    if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType, out List<RenderTargetInfo> renderTargetInfos))
                    {
                        CachedTypeToRenderTargetInfos[reflectedType] = renderTargetInfos = new List<RenderTargetInfo>();
                    }

                    if (!renderTargetInfos.Contains(info))
                    {
                        renderTargetInfos.Add(info);
                    }
                }
            }
#endif

            foreach (Type reflectedType in TypeCache.GetTypesWithAttribute<AbsComponentHeaderAttribute>())
            {
                foreach (AbsComponentHeaderAttribute attribute in ReflectCache.GetCustomAttributes<AbsComponentHeaderAttribute>(reflectedType))
                {
                    RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Class, null, -1);

                    // subclass
                    foreach (KeyValuePair<Type, List<RenderTargetInfo>> cachedKv in CachedTypeToRenderTargetInfos)
                    {
                        if (reflectedType.IsAssignableFrom(cachedKv.Key) && reflectedType != cachedKv.Key)
                        {
                            cachedKv.Value.Add(info);
                        }
                    }

                    if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType, out List<RenderTargetInfo> renderTargetInfos))
                    {
                        CachedTypeToRenderTargetInfos[reflectedType] = renderTargetInfos = new List<RenderTargetInfo>();
                    }

                    if (!renderTargetInfos.Contains(info))
                    {
                        renderTargetInfos.Add(info);
                    }
                }
            }

            foreach (Type type in CachedTypeToRenderTargetInfos.Keys.ToArray())
            {
                CachedTypeToRenderTargetInfos[type] = ReCreateOrdered(CachedTypeToRenderTargetInfos[type]).ToList();
            }
        }

        private static bool DrawMethod(Rect rectangle, Object[] targets)
        {
            if (rectangle.x < 0)
            {
                return false;
            }

            // EditorGUI.DrawRect(rectangle, Color.blue);

            Object firstTarget = targets[0];
            Type firstTargetType = firstTarget.GetType();
            if(!CachedTypeToRenderTargetInfos.TryGetValue(firstTargetType, out List<RenderTargetInfo> renderTargetInfos))
            {
                CachedTypeToRenderTargetInfos[firstTargetType] = renderTargetInfos = ReCreateOrdered(CachedTypeToRenderTargetInfos
                        .Where(each => each.Key.IsAssignableFrom(firstTargetType))
                        .SelectMany(each => each.Value)
                        .ToList()
                    )
                    .ToList();
            }

            HashSet<ISearchable> removeSaintsEditor = new HashSet<ISearchable>();

            // Debug.Log($"SearchableSaintsEditors={SearchableSaintsEditors.Count}");

            foreach (ISearchable searchableSaintsEditor in SearchableSaintsEditors)
            {
                // Debug.Log(searchableSaintsEditor);
                if (!(Object)searchableSaintsEditor)
                {
                    removeSaintsEditor.Add(searchableSaintsEditor);
                    continue;
                }

                if (!Util.GetIsEqual(searchableSaintsEditor.target, firstTarget))
                {
                    continue;
                }

                Rect useRect = new Rect(rectangle);
                rectangle.x -= rectangle.height;
                rectangle.xMax -= rectangle.height;

                // Rect actualRect = new Rect(useRect)
                // {
                //     x = useRect.x + 1,
                //     width = useRect.width - 2,
                //     y = useRect.y + 1,
                //     height = useRect.height - 2,
                // };

                GUIStyle btnStyle = CacheAndUtil.GetIconButtonStyle();

                if (GUI.Button(useRect, GUIContent.none, btnStyle))
                {
                    searchableSaintsEditor.OnHeaderButtonClick();
                }

                string richLabel = searchableSaintsEditor.GetRichLabel();
                if (!CacheAndUtil.ParsedXmlCache.TryGetValue(richLabel,
                        out IReadOnlyList<RichTextDrawer.RichTextChunk> chunks))
                {
                    CacheAndUtil.ParsedXmlCache[richLabel] = chunks = RichTextDrawer.ParseRichXml(richLabel, "", null, null, firstTarget).ToArray();
                }

                CacheAndUtil.GetCachedRichTextDrawer().DrawChunks(useRect, GUIContent.none, chunks);
            }

            SearchableSaintsEditors.ExceptWith(removeSaintsEditor);

            if (renderTargetInfos.Count == 0)
            {
                return false;
            }

#if UNITY_2023_1_OR_NEWER
            string title = ObjectNames.GetInspectorTitle(firstTarget, targets.Length > 1);
#else
            string title = ObjectNames.GetInspectorTitle(firstTarget);
#endif
            float titleWidth = EditorStyles.largeLabel.CalcSize(new GUIContent(title)).x;
            bool isHierarchyInspecting = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(firstTarget));

            // Debug.Log($"{title}: {targets[0]}/{isHierarchyInspecting}");

            float prefixWidth = isHierarchyInspecting? 60: 45;
            const float gap = 10;

            rectangle.x = rectangle.xMax;
            rectangle.width = 0;

            HeaderArea headerArea = new HeaderArea(
                rectangle.y,
                rectangle.height,
                prefixWidth, prefixWidth + titleWidth,
                prefixWidth + gap + titleWidth, rectangle.xMax,

                prefixWidth + gap + titleWidth, Array.Empty<Rect>());

            float xMax = rectangle.xMax;
            float xMin = prefixWidth + gap + titleWidth;

            rectangle.x = xMin;
            rectangle.width = xMax - xMin;


            float xLeft = headerArea.SpaceStartX;
            float xRight = rectangle.xMax;

            string preLeftGrouBy = null;
            List<Rect> preLeftUsedRects = new List<Rect>();
            string preRightGrouBy = null;
            List<Rect> preRightUsedRects = new List<Rect>();

            foreach (RenderTargetInfo renderTargetInfo in renderTargetInfos)
            {
                switch (renderTargetInfo.Attribute)
                {
                    case HeaderButtonAttribute headerButtonAttribute:
                    {
                        if (headerButtonAttribute.IsLeft)
                        {
                            preLeftGrouBy = null;
                            if(preLeftUsedRects.Count > 0)
                            {
                                xLeft = Mathf.Max(xLeft, preLeftUsedRects.Max(each => each.xMax));
                                preLeftUsedRects.Clear();
                            }
                        }
                        else
                        {
                            preRightGrouBy = null;
                            if(preRightUsedRects.Count > 0)
                            {
                                xRight = Mathf.Min(xRight, preRightUsedRects.Min(each => each.x));
                                preRightUsedRects.Clear();
                            }
                        }

                        float x = headerButtonAttribute.IsLeft ? xLeft : xRight;
                        (bool buttonUsed, HeaderUsed buttonHeaderUsed) = HeaderButtonDrawer.Draw(
                            firstTarget,
                            headerArea.EditorWrap(x, Array.Empty<Rect>()),
                            headerButtonAttribute,
                            renderTargetInfo
                        );
                        if (buttonUsed)
                        {
                            if (headerButtonAttribute.IsLeft)
                            {
                                xLeft = buttonHeaderUsed.UsedRect.xMax;

                            }
                            else
                            {
                                xRight = buttonHeaderUsed.UsedRect.x;

                            }
                        }

                        break;
                    }
                    case HeaderDrawAttribute headerDrawAttribute:
                    {
                        string groupBy = headerDrawAttribute.GroupBy;
                        IReadOnlyList<Rect> usedRects = Array.Empty<Rect>();
                        if (!string.IsNullOrEmpty(groupBy))
                        {
                            // get used rect
                            usedRects = headerDrawAttribute.IsLeft ? preLeftUsedRects : preRightUsedRects;

                            // check if it's new group
                            if (headerDrawAttribute.IsLeft)
                            {
                                if (preLeftGrouBy != headerDrawAttribute.GroupBy && preLeftUsedRects.Count > 0)
                                {
                                    preLeftGrouBy = headerDrawAttribute.GroupBy;
                                    xLeft = Mathf.Max(xLeft, preLeftUsedRects.Max(each => each.xMax));
                                    preLeftUsedRects.Clear();
                                }
                            }
                            else
                            {
                                if (preRightGrouBy != headerDrawAttribute.GroupBy && preRightUsedRects.Count > 0)
                                {
                                    preRightGrouBy = headerDrawAttribute.GroupBy;
                                    xRight = Mathf.Min(xRight, preRightUsedRects.Min(each => each.x));
                                    preRightUsedRects.Clear();
                                }
                            }
                        }
                        else
                        {
                            if (headerDrawAttribute.IsLeft)
                            {
                                preLeftGrouBy = null;
                                if(preLeftUsedRects.Count > 0)
                                {
                                    xLeft = Mathf.Max(xLeft, preLeftUsedRects.Max(each => each.xMax));
                                    preLeftUsedRects.Clear();
                                }
                            }
                            else
                            {
                                preRightGrouBy = null;
                                if(preRightUsedRects.Count > 0)
                                {
                                    xRight = Mathf.Min(xRight, preRightUsedRects.Min(each => each.x));
                                    preRightUsedRects.Clear();
                                }
                            }
                        }

                        (bool used, HeaderUsed headerUsed) = HeaderDrawDrawer.Draw(
                            firstTarget,
                            headerArea.EditorWrap(headerDrawAttribute.IsLeft ? xLeft : xRight, usedRects),
                            renderTargetInfo
                        );

                        if (used)
                        {
                            if (string.IsNullOrEmpty(groupBy))
                            {
                                if (headerDrawAttribute.IsLeft)
                                {
                                    xLeft = headerUsed.UsedRect.xMax;
                                }
                                else
                                {
                                    xRight = headerUsed.UsedRect.x;
                                }
                            }
                            else
                            {
                                if (headerDrawAttribute.IsLeft)
                                {
                                    preLeftUsedRects.Add(headerUsed.UsedRect);
                                    preLeftGrouBy = headerDrawAttribute.GroupBy;
                                }
                                else
                                {
                                    preRightUsedRects.Add(headerUsed.UsedRect);
                                    preRightGrouBy = headerDrawAttribute.GroupBy;
                                }
                            }
                        }
                    }
                        break;
                    case HeaderLabelAttribute headerLabelAttribute:
                    {
                        string groupBy = headerLabelAttribute.GroupBy;
                        IReadOnlyList<Rect> usedRects = Array.Empty<Rect>();
                        if (!string.IsNullOrEmpty(groupBy))
                        {
                            // get used rect
                            usedRects = headerLabelAttribute.IsLeft ? preLeftUsedRects : preRightUsedRects;

                            // check if it's new group
                            if (headerLabelAttribute.IsLeft)
                            {
                                if (preLeftGrouBy != headerLabelAttribute.GroupBy && preLeftUsedRects.Count > 0)
                                {
                                    preLeftGrouBy = headerLabelAttribute.GroupBy;
                                    xLeft = Mathf.Max(xLeft, preLeftUsedRects.Max(each => each.xMax));
                                    preLeftUsedRects.Clear();
                                }
                            }
                            else
                            {
                                if (preRightGrouBy != headerLabelAttribute.GroupBy && preRightUsedRects.Count > 0)
                                {
                                    preRightGrouBy = headerLabelAttribute.GroupBy;
                                    xRight = Mathf.Min(xRight, preRightUsedRects.Min(each => each.x));
                                    preRightUsedRects.Clear();
                                }
                            }
                        }
                        else
                        {
                            if (headerLabelAttribute.IsLeft)
                            {
                                preLeftGrouBy = null;
                                if(preLeftUsedRects.Count > 0)
                                {
                                    xLeft = Mathf.Max(xLeft, preLeftUsedRects.Max(each => each.xMax));
                                    preLeftUsedRects.Clear();
                                }
                            }
                            else
                            {
                                preRightGrouBy = null;
                                if(preRightUsedRects.Count > 0)
                                {
                                    xRight = Mathf.Min(xRight, preRightUsedRects.Min(each => each.x));
                                    preRightUsedRects.Clear();
                                }
                            }
                        }

                        (bool used, HeaderUsed headerUsed) = HeaderLabelDrawer.Draw(
                            firstTarget,
                            headerArea.EditorWrap(headerLabelAttribute.IsLeft ? xLeft : xRight, usedRects),
                            headerLabelAttribute,
                            renderTargetInfo
                        );

                        if (used)
                        {
                            if (string.IsNullOrEmpty(groupBy))
                            {
                                if (headerLabelAttribute.IsLeft)
                                {
                                    xLeft = headerUsed.UsedRect.xMax;
                                }
                                else
                                {
                                    xRight = headerUsed.UsedRect.x;
                                }
                            }
                            else
                            {
                                if (headerLabelAttribute.IsLeft)
                                {
                                    preLeftUsedRects.Add(headerUsed.UsedRect);
                                    preLeftGrouBy = headerLabelAttribute.GroupBy;
                                }
                                else
                                {
                                    preRightUsedRects.Add(headerUsed.UsedRect);
                                    preRightGrouBy = headerLabelAttribute.GroupBy;
                                }
                            }
                        }
                    }
                        break;
                }
            }

            return false;
        }

        private static IEnumerable<RenderTargetInfo> ReCreateOrdered(List<RenderTargetInfo> allInfos)
        {
            List<RenderTargetInfo> leftInfos = new List<RenderTargetInfo>(allInfos.Count);
            List<RenderTargetInfo> rightInfos = new List<RenderTargetInfo>(allInfos.Count);
            foreach (RenderTargetInfo renderTargetInfo in allInfos)
            {
                if (renderTargetInfo.Attribute.IsLeft)
                {
                    leftInfos.Add(renderTargetInfo);
                }
                else
                {
                    rightInfos.Add(renderTargetInfo);
                }
            }

            leftInfos.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            rightInfos.Sort((a, b) => -a.SortOrder.CompareTo(b.SortOrder));

            return ReOrdered(rightInfos).Concat(ReOrdered(leftInfos));
        }

        private static IEnumerable<RenderTargetInfo> ReOrdered(List<RenderTargetInfo> infos)
        {
            List<int> skipIndices = new List<int>(infos.Count);

            for (int index = 0; index < infos.Count; index++)
            {
                if (skipIndices.Contains(index))
                {
                    continue;
                }

                RenderTargetInfo info = infos[index];
                string groupBy = info.Attribute.GroupBy;
                yield return info;

                // ReSharper disable once InvertIf
                if (!string.IsNullOrEmpty(groupBy))
                {
                    yield return info;
                    for (int postIndex = index + 1; postIndex < infos.Count; postIndex++)
                    {
                        RenderTargetInfo postInfo = infos[postIndex];
                        // ReSharper disable once InvertIf
                        if (postInfo.Attribute.GroupBy == groupBy)
                        {
                            yield return postInfo;
                            skipIndices.Add(postIndex);
                        }
                    }
                }
            }
        }

        public static bool AddAttributeIfNot(AbsComponentHeaderAttribute attribute, MemberInfo memberInfo, object target, int order)
        {
            Type reflectedType = target.GetType();
            if (memberInfo == null)
            {
                RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Class, null, order);
                if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType,
                        out List<RenderTargetInfo> renderTargetInfos))
                {
                    CachedTypeToRenderTargetInfos[reflectedType] =
                        renderTargetInfos = new List<RenderTargetInfo>();
                }

                if (renderTargetInfos.Contains(info))
                {
                    return false;
                }

                renderTargetInfos.Add(info);
                return true;
            }

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                    switch (attribute)
                    {
                        case HeaderButtonAttribute _:
                        {
                            if (!HeaderButtonDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                            {
                                return false;
                            }
                        }
                            break;
                        case HeaderDrawAttribute _:
                        {
                            if (!HeaderDrawDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                            {
                                return false;
                            }
                        }
                            break;
                        case HeaderLabelAttribute _:
                        {
                            if (!HeaderLabelDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                            {
                                return false;
                            }
                        }
                            break;
                    }

                    RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Method, methodInfo, order);

                    if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType,
                            out List<RenderTargetInfo> renderTargetInfos))
                    {
                        CachedTypeToRenderTargetInfos[reflectedType] = renderTargetInfos = new List<RenderTargetInfo>();
                    }
                    if (renderTargetInfos.Contains(info))
                    {
                        return false;
                    }
                    renderTargetInfos.Add(info);

                    return true;
                }
                case MemberTypes.Field:
                {
                    if(attribute is HeaderLabelAttribute)
                    {
                        RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Field, memberInfo, order);
                        if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType,
                                out List<RenderTargetInfo> renderTargetInfos))
                        {
                            CachedTypeToRenderTargetInfos[reflectedType] =
                                renderTargetInfos = new List<RenderTargetInfo>();
                        }

                        if (renderTargetInfos.Contains(info))
                        {
                            return false;
                        }

                        renderTargetInfos.Add(info);
                        return true;
                    }
                }
                    break;
                case MemberTypes.Property:
                {
                    if (attribute is HeaderLabelAttribute)
                    {
                        RenderTargetInfo info = new RenderTargetInfo(attribute, MemberType.Property, memberInfo, order);
                        if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType,
                                out List<RenderTargetInfo> renderTargetInfos))
                        {
                            CachedTypeToRenderTargetInfos[reflectedType] =
                                renderTargetInfos = new List<RenderTargetInfo>();
                        }

                        if (renderTargetInfos.Contains(info))
                        {
                            return false;
                        }

                        renderTargetInfos.Add(info);
                        return true;
                    }
                }
                    break;
            }

            return false;
        }

        public static void RefreshAddAttributeIfNot(Type targetType)
        {
            CachedTypeToRenderTargetInfos[targetType] = ReCreateOrdered(CachedTypeToRenderTargetInfos[targetType]).ToList();
        }

        public static void HelperUpdate()
        {
            if (!_initLoad)
            {
                return;
            }

            HeaderButtonDrawer.Update();
        }

        private static readonly HashSet<ISearchable> SearchableSaintsEditors = new HashSet<ISearchable>();

        public static void SaintsEditorEnqueueSearchable(SaintsEditor saintsEditor)
        {
            SearchableSaintsEditors.Add(saintsEditor);
        }
    }
}
