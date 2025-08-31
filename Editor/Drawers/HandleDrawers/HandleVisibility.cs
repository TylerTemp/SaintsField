using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SceneViewPickerDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers
{
    public static class HandleVisibility
    {
        private static readonly Dictionary<string, bool> _idToHide = new Dictionary<string, bool>();

        public static bool IsHidden(string id)
        {
            // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
            if (_idToHide.TryGetValue(id, out bool isHidden))
            {
                return isHidden;
            }

            bool prefsIsHidden = EditorPrefs.GetBool(id);
            return _idToHide[id] = prefsIsHidden;
        }

        private static void SetHidden(string id, bool hidden)
        {
            if (hidden)
            {
                EditorPrefs.SetBool(id, true);
                _idToHide[id] = true;
            }
            else
            {
                EditorPrefs.DeleteKey(id);
                _idToHide.Remove(id);
            }
        }

        private readonly struct Info: IEquatable<Info>
        {
            public readonly string Id;
            public readonly string PropPath;
            public readonly string ContainerName;
            public readonly Texture2D Icon;

            public Info(string id, string propPath, string containerName, Texture2D icon)
            {
                Id = id;
                PropPath = propPath.Replace(".Array.data[", "[");
                ContainerName = containerName;
                Icon = icon;
            }

            public bool Equals(Info other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                // ReSharper disable once Unity.BurstLoadingManagedType
                return obj is Info other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Id != null ? Id.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return Id;
            }
        }

        private static readonly List<Info> InView = new List<Info>();

        private static bool IsListening;

        public static void SetInView(string id, string propPath, string containerName, Texture2D icon)
        {
            Info info = new Info(id, propPath, containerName, icon);
            if (!InView.Contains(info))
            {
                InView.Add(info);
            }

            if (!IsListening)
            {
                IsListening = true;
                SceneView.duringSceneGui -= OnSceneGUI;
                SceneView.duringSceneGui += OnSceneGUI;

                Selection.selectionChanged -= SelectionChanged;
                Selection.selectionChanged += SelectionChanged;
            }
        }

        private static void SelectionChanged()
        {
            _showSelectingPanel = false;
            ClearMenu();
        }

        public static void SetOutView(string id)
        {
            // Info info = new Info(id, name, containerName);
            InView.RemoveAll(each => each.Id == id);
        }

        private static string _selectingPanelSearching = "";
        private static float _selectingPanelWidth = -1f;
        private static Vector2 _scrollPos;

        private readonly struct ShowInfo
        {
            public readonly Info Info;
            public readonly Texture2D Icon;
            public readonly string ItemDisplay;
            public readonly bool Hidden;

            public ShowInfo(Info info, Texture2D icon, string itemDisplay, bool hidden)
            {
                Info = info;
                Icon = icon;
                ItemDisplay = itemDisplay;
                Hidden = hidden;
            }
        }

        private static void ClearMenu()
        {
            _selectingPanelWidth = -1f;
            _selectingPanelSearching = "";
        }

        private static Texture2D _eyeIcon;
        private static Texture2D _eyeSlashIcon;

        private static Texture2D GetEyeIcon()
        {
            if (_eyeIcon is null)
            {
                return _eyeIcon = Util.LoadResource<Texture2D>("eye.png");
            }

            return _eyeIcon;
        }

        private static Texture2D GetEyeSlashIcon()
        {
            if (_eyeSlashIcon is null)
            {
                return _eyeSlashIcon = Util.LoadResource<Texture2D>("eye-slash-gray.png");
            }

            return _eyeSlashIcon;
        }

        private static GUIStyle _leftButtonStyleCache;

        private static GUIStyle LeftButtonStyle
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_leftButtonStyleCache == null)
                {
                    _leftButtonStyleCache = new GUIStyle(GUI.skin.button)
                    {
                        border = new RectOffset(0, 0, 0, 0),
                        alignment = TextAnchor.MiddleLeft,
                        richText = true,
                        normal = { textColor = Color.white },
                        wordWrap = true,
                        // padding = new RectOffset(120, 0, 0, 0),
                        // margin = new RectOffset(100, 0, 0, 0),
                        // normal = { background = null },
                        // hover = { background = null },
                        // active = { background = null },
                        // focused = { background = null },
                    };
                }

                return _leftButtonStyleCache;
            }
        }

        private static bool _showSelectingPanel;
        private static Vector2 _selectingPanelMouseFrozenPos;

        private static Vector2 _lastRightClickPos;
        private static double _lastRightClickTime;

        private static void OnSceneGUI(SceneView sv)
        {
            if (InView.Count == 0)
            {
                return;
            }

            if (_showSelectingPanel)
            {
                DrawMenu();
            }

            if (Event.current.alt ||
                Event.current.control)
            {
                return;
            }

            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                _lastRightClickPos = Event.current.mousePosition;
                _lastRightClickTime = EditorApplication.timeSinceStartup;
                // Debug.Log($"dn {_lastRightClickPos}/{_lastRightClickTime}");
                return;
            }

            if (Event.current.button != 1 || Event.current.type != EventType.MouseUp)
            {
                return;
            }

            if (!((_lastRightClickPos - Event.current.mousePosition).sqrMagnitude < 0.1f) ||
                !(EditorApplication.timeSinceStartup - _lastRightClickTime < 0.1f))
            {
                // Debug.Log($"up {Event.current.mousePosition}/{EditorApplication.timeSinceStartup}: {(_lastRightClickPos - Event.current.mousePosition).sqrMagnitude}, {EditorApplication.timeSinceStartup - _lastRightClickTime}");
                return;
            }

            if (!_showSelectingPanel)
            {
                _selectingPanelMouseFrozenPos = Event.current.mousePosition;
                ClearMenu();
            }

            _showSelectingPanel = !_showSelectingPanel;
            Event.current.Use();
            sv.Repaint();
        }

        private static void DrawMenu()
        {
            List<Info> showTargets;
            if (string.IsNullOrEmpty(_selectingPanelSearching))
            {
                showTargets = InView;
            }
            else
            {
                showTargets = new List<Info>(InView.Count);
                foreach (Info findTargetRecord in InView)
                {
                    if(SearchMatch(findTargetRecord, _selectingPanelSearching))
                    {
                        showTargets.Add(findTargetRecord);
                    }
                }
            }

            float calcWidth = 100;
            float height = EditorGUIUtility.singleLineHeight + 4;
            List<ShowInfo> showInfos = new List<ShowInfo>(showTargets.Count);
            foreach (Info showTarget in showTargets)
            {
                string itemDisplay = $"{showTarget.PropPath} <color=grey>({showTarget.ContainerName})</color>";
                bool isHidden = IsHidden(showTarget.Id);
                ShowInfo showInfo = new ShowInfo(
                    showTarget,
                    isHidden? GetEyeSlashIcon(): GetEyeIcon(),
                    itemDisplay,
                    isHidden
                );
                showInfos.Add(showInfo);

                if(_selectingPanelWidth < 0)
                {
                    float thisWidth = GUI.skin.label.CalcSize(new GUIContent($"{showTarget.PropPath} ({showTarget.ContainerName})")).x
                        + EditorGUIUtility.singleLineHeight * 2 + 6;
                    if (thisWidth > calcWidth)
                    {
                        calcWidth = thisWidth;
                    }
                }

                height += EditorGUIUtility.singleLineHeight;
            }
            if (_selectingPanelWidth < 0)
            {
                _selectingPanelWidth = calcWidth;
            }

            float useX = _selectingPanelMouseFrozenPos.x - _selectingPanelWidth / 10;
            if (useX + _selectingPanelWidth > Screen.width)
            {
                useX = Screen.width - _selectingPanelWidth;
            }
            else if (useX < 0)
            {
                useX = 0;
            }

            float useY = _selectingPanelMouseFrozenPos.y - 5;
            float viewHeight = Mathf.Min(600, height, Screen.height - useY - 100);
            if (viewHeight < 100)
            {
                viewHeight = 100;
            }

            using (new HandlesBeginGUIScoop())
            {
                GUI.Window("HandleVisibilityMenu".GetHashCode(), new Rect(useX, useY, _selectingPanelWidth, viewHeight), _ =>
                {
                    Rect search = new Rect(2, 2, _selectingPanelWidth - 4, EditorGUIUtility.singleLineHeight);
                    GUI.SetNextControlName("HandleVisibilityMenuSearchField");
                    _selectingPanelSearching = GUI.TextField(search, _selectingPanelSearching);
                    // Content of window here
                    // GUILayout.Button("A Button");

                    using (GUIBeginScrollViewScoop scrollView = new GUIBeginScrollViewScoop(
                               new Rect(0, EditorGUIUtility.singleLineHeight + 4, _selectingPanelWidth, viewHeight - (EditorGUIUtility.singleLineHeight + 4)),
                               _scrollPos,
                               new Rect(0, EditorGUIUtility.singleLineHeight + 4, _selectingPanelWidth - 15, height - (EditorGUIUtility.singleLineHeight + 4)))
                           )
                    {
                        _scrollPos = scrollView.ScrollPosition;
                        int index = 0;
                        foreach (ShowInfo findTargetRecord in showInfos)
                        {
                            Rect buttonRect = new Rect(0,
                                EditorGUIUtility.singleLineHeight + 4 + EditorGUIUtility.singleLineHeight * index,
                                _selectingPanelWidth, EditorGUIUtility.singleLineHeight);

                            if (viewHeight < height)
                            {
                                buttonRect.width -= 15;
                            }

                            if (GUI.Button(buttonRect, GUIContent.none, LeftButtonStyle))
                            {
                                // Debug.Log(findTargetRecord.ItemDisplay);
                                SetHidden(findTargetRecord.Info.Id, !findTargetRecord.Hidden);
                                return;
                            }

                            GUI.DrawTexture(new Rect(buttonRect)
                            {
                                x = buttonRect.x + 4,
                                y = buttonRect.y + 1,
                                width = EditorGUIUtility.singleLineHeight,
                                height = buttonRect.height - 2,
                            }, findTargetRecord.Icon);

                            if(findTargetRecord.Info.Icon)
                            {
                                GUI.DrawTexture(new Rect(buttonRect)
                                {
                                    x = buttonRect.x + 4 + EditorGUIUtility.singleLineHeight + 2,
                                    y = buttonRect.y + 1,
                                    width = EditorGUIUtility.singleLineHeight,
                                    height = EditorGUIUtility.singleLineHeight,
                                }, findTargetRecord.Info.Icon);
                            }

                            GUI.Label(new Rect(buttonRect)
                            {
                                x = buttonRect.x + EditorGUIUtility.singleLineHeight * 2 + 6,
                                width = buttonRect.width - EditorGUIUtility.singleLineHeight * 2,
                            }, findTargetRecord.ItemDisplay, new GUIStyle(GUI.skin.label)
                            {
                                // wordWrap = true,
                                richText = true,
                                // normal = { textColor = Color.white },
                            });

                            // if(findTargetRecord.Info.Icon)
                            // {
                            //     GUI.DrawTexture(new Rect(buttonRect)
                            //     {
                            //         x = buttonRect.x + 22,
                            //         width = SaintsPropertyDrawer.SingleLineHeight,
                            //     }, findTargetRecord.Info.Icon);
                            // }

                            index++;
                        }
                    }

                    if (Event.current.type == EventType.Repaint) {
                        GUI.FocusControl("HandleVisibilityMenuSearchField");
                        // shouldFocus = false;
                    }
                }, "Title");
            }
        }

        private static bool SearchMatch(Info findTargetRecord, string selectingPanelSearching)
        {
            string beSearched = $"{findTargetRecord.PropPath} {findTargetRecord.ContainerName}";
            return selectingPanelSearching
                .ToLower()
                .Split(' ')
                .All(searchSeg => beSearched.Contains(searchSeg));
        }
    }
}
