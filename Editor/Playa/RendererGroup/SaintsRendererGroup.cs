using System;
using System.Collections.Generic;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public partial class SaintsRendererGroup: ISaintsRendererGroup
    {
        public class Config
        {
            public bool KeepGrouping;

            public ELayout ELayout;
            public bool IsDoTween;
            public float MarginTop;
            public float MarginBottom;
            public IReadOnlyList<ISaintsLayoutToggle> Toggles;
        }

        private int _curSelected;

        private readonly Dictionary<string, List<ISaintsRenderer>> _groupIdToRenderer =
            new Dictionary<string, List<ISaintsRenderer>>();
        private readonly List<(string groupPath, ISaintsRenderer renderer)> _renderers =
            new List<(string groupPath, ISaintsRenderer renderer)>();

        private readonly List<string> _orderedKeys = new List<string>();  // no OrderedDict can use...

        private readonly string _groupPath;
        private readonly ELayout _eLayout;
        private readonly Config _config;

        private GUIStyle _foldoutSmallStyle;
        private GUIStyle _titleLabelStyle;

        private bool _foldout;

        private readonly object _containerObject;

        private readonly List<ToggleCheckInfo> _toggleCheckInfos;

        public SaintsRendererGroup(string groupPath, Config config, object containerObject)
        {
            _groupPath = groupPath;
            _config = config;
            _eLayout = config.ELayout;
            _foldout = !config.ELayout.HasFlagFast(ELayout.Collapse);
            _containerObject = containerObject;

            List<ToggleCheckInfo> toggleCheckInfos = new List<ToggleCheckInfo>(_config.Toggles.Count);

            foreach (ISaintsLayoutToggle configToggle in _config.Toggles)
            {
                switch (configToggle)
                {
                    case LayoutEnableIfAttribute layoutEnableIfAttribute:
                        // layoutEnableIf.Add(layoutEnableIfAttribute);
                        // Debug.Log(layoutEnableIfAttribute);
                        toggleCheckInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Enable,
                            layoutEnableIfAttribute.ConditionInfos,
                            _containerObject
                        ));
                        break;
                    case LayoutReadOnlyAttribute layoutReadOnlyAttribute:
                        toggleCheckInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Disable,
                            layoutReadOnlyAttribute.ConditionInfos,
                            _containerObject
                        ));
                        break;

                    case LayoutHideIfAttribute layoutHideIfAttribute:
                        toggleCheckInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Hide,
                            layoutHideIfAttribute.ConditionInfos,
                            _containerObject
                        ));
                        break;
                    case LayoutShowIfAttribute layoutShowIfAttribute:
                        toggleCheckInfos.Add(new ToggleCheckInfo
                        (
                            ToggleType.Show,
                            layoutShowIfAttribute.ConditionInfos,
                            _containerObject
                        ));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(configToggle), configToggle, null);
                }
            }

            _toggleCheckInfos = toggleCheckInfos;
        }

        public void Add(string groupPath, ISaintsRenderer renderer)
        {
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string lastId = groupPath.Substring(groupPath.LastIndexOf('/') + 1);

            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            // if(_curSelected == null)
            // {
            //     _curSelected = lastId;
            // }

            if(!_groupIdToRenderer.TryGetValue(lastId, out List<ISaintsRenderer> renderers))
            {
                _groupIdToRenderer[lastId] = renderers = new List<ISaintsRenderer>();
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_EDITOR_LAYOUT
                Debug.Log($"Add Key to {_groupPath}: {lastId} of {groupPath}");
#endif
                _orderedKeys.Add(lastId);
            }

            renderers.Add(renderer);
            _renderers.Add((groupPath, renderer));
        }

        public void OnDestroy()
        {
            foreach ((string _, ISaintsRenderer renderer) in _renderers)
            {
                renderer.OnDestroy();
            }
        }

        public override string ToString() => $"<Group path={_groupPath} layout={_eLayout}/>";

        private static bool IsFancyBox(ELayout eLayout) => eLayout.HasFlagFast(ELayout.Background) || eLayout.HasFlagFast(ELayout.Tab);

        private static bool NeedIndentCheck(ELayout eLayout) => IsFancyBox(eLayout) ||
                                                                eLayout.HasFlagFast(ELayout.Foldout) ||
                                                                eLayout.HasFlagFast(ELayout.Collapse);

    }
}
