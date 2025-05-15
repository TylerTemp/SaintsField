using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.ComponentHeader;
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

        public readonly struct RenderTargetInfo : IEquatable<RenderTargetInfo>
        {
            // public readonly Type Type;
            // public readonly bool FromLeft;
            // public readonly string Group;
            public readonly AbsComponentHeaderAttribute Attribute;
            public readonly MethodInfo MethodInfo;
            public readonly int MetadataToken;

            public RenderTargetInfo(AbsComponentHeaderAttribute attribute, MethodInfo methodInfo, int metadataToken)
            {
                Attribute = attribute;
                MethodInfo = methodInfo;
                MetadataToken = metadataToken;
            }

            public bool Equals(RenderTargetInfo other)
            {
                return Equals(Attribute, other.Attribute) && Equals(MethodInfo, other.MethodInfo);
            }

            public override bool Equals(object obj)
            {
                return obj is RenderTargetInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Util.CombineHashCode(Attribute, MethodInfo);
            }

            public override string ToString() => $"<RenderTargetInfo {MethodInfo.Name} {Attribute.GetType()} {Attribute.IsLeft} {Attribute.GroupBy} />";
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

                AbsComponentHeaderAttribute attribute = ReflectCache.GetCustomAttributes<AbsComponentHeaderAttribute>(methodInfo)[0];

                switch (attribute)
                {
                    case HeaderButtonAttribute:
                    {
                        if (!HeaderButtonDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                        {
                            continue;
                        }
                    }
                        break;
                    case HeaderDrawAttribute:
                    {
                        if(!HeaderDrawDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                        {
                            continue;
                        }
                    }
                        break;
                }

                RenderTargetInfo info = new RenderTargetInfo(attribute, methodInfo, methodInfo.MetadataToken);

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

            if (renderTargetInfos.Count == 0)
            {
                return false;
            }

            string title = ObjectNames.GetInspectorTitle(firstTarget, targets.Length > 1);
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

            // rectangle.x -= 40;
            // rectangle.width += 40;
            // Debug.Log(rectangle.y);

            // EditorGUI.DrawRect(rectangle, Color.blue * new Color(1, 1, 1, 0.3f));
            // EditorGUI.DrawRect(new Rect(headerArea.SpaceStartX, headerArea.Y, headerArea.SpaceEndX - headerArea.SpaceStartX, headerArea.Height), Color.blue * new Color(1, 1, 1, 0.3f));

            // Rect titleRect = new Rect(rectangle)
            // {
            //     x = prefixWidth,
            //     width = titleWidth,
            // };
            // EditorGUI.DrawRect(titleRect, Color.yellow * new Color(1, 1, 1, 0.2f));
            // EditorGUI.DrawRect(new Rect(headerArea.TitleStartX, headerArea.Y, headerArea.TitleEndX - headerArea.TitleStartX, headerArea.Height), Color.yellow * new Color(1, 1, 1, 0.2f));
            // EditorGUI.LabelField(rectangle, title);

            // List<Rect> nullLeftUsed = new List<Rect>();
            // List<Rect> nullRightUsed = new List<Rect>();
            // List<RenderTargetInfo> orderedRenderTargetInfos = new List<RenderTargetInfo>(renderTargetInfos.Count);
            // List<RenderTargetInfo> leftInfos = new List<RenderTargetInfo>(renderTargetInfos.Count);
            // List<RenderTargetInfo> rightInfos = new List<RenderTargetInfo>(renderTargetInfos.Count);
            // foreach (RenderTargetInfo renderTargetInfo in renderTargetInfos)
            // {
            //     if (renderTargetInfo.Attribute.IsLeft)
            //     {
            //         leftInfos.Add(renderTargetInfo);
            //     }
            //     else
            //     {
            //         rightInfos.Insert(0, renderTargetInfo);
            //     }
            // }


            // leftInfos.Sort((a, b) => string.Compare(a.Attribute.GroupBy, b.Attribute.GroupBy, StringComparison.Ordinal));
            // rightInfos.Sort((a, b) => string.Compare(a.Attribute.GroupBy, b.Attribute.GroupBy, StringComparison.Ordinal));

            float xLeft = headerArea.SpaceStartX;
            float xRight = rectangle.xMax;

            // Dictionary<string, List<Rect>> leftGroupToUsedRects = new Dictionary<string, List<Rect>>();
            // Dictionary<string, List<Rect>> rightGroupToUsedRects = new Dictionary<string, List<Rect>>();

            string preLeftGrouBy = null;
            List<Rect> preLeftUsedRects = new List<Rect>();
            string preRightGrouBy = null;
            List<Rect> preRightUsedRects = new List<Rect>();

            // rightInfos.AddRange(leftInfos);


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

            leftInfos.Sort((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
            rightInfos.Sort((a, b) => -a.MetadataToken.CompareTo(b.MetadataToken));

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

        public static bool AddAttributeIfNot(AbsComponentHeaderAttribute attribute, MethodInfo methodInfo, object target, int order)
        {
            Type reflectedType = target.GetType();
            switch (attribute)
            {
                case HeaderButtonAttribute:
                {
                    if (!HeaderButtonDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                    {
                        return false;
                    }
                }
                    break;
                case HeaderDrawAttribute:
                {
                    if(!HeaderDrawDrawer.ValidateMethodInfo(methodInfo, reflectedType))
                    {
                        return false;
                    }
                }
                    break;
            }

            RenderTargetInfo info = new RenderTargetInfo(attribute, methodInfo, order);

            if (!CachedTypeToRenderTargetInfos.TryGetValue(reflectedType, out List<RenderTargetInfo> renderTargetInfos))
            {
                CachedTypeToRenderTargetInfos[reflectedType] = renderTargetInfos = new List<RenderTargetInfo>();
            }

            if (renderTargetInfos.Contains(info))
            {
                return false;
            }

            // Debug.Log(methodInfo.Name);
            renderTargetInfos.Add(info);
            return true;

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
    }
}
