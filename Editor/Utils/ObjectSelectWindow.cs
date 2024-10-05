using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils
{
    public abstract class ObjectSelectWindow: EditorWindow
    {
        protected abstract bool AllowScene { get; }
        protected abstract bool AllowAssets { get; }

        protected abstract string Error { get; }

        private class GUIBackgroundScoop : IDisposable
        {
            private readonly bool _changed;
            private readonly Color _oriColor;

            public GUIBackgroundScoop(bool change, Color color)
            {
                _changed = change;
                if(change)
                {
                    _oriColor = GUI.backgroundColor;
                    GUI.backgroundColor = color;
                }
            }

            public void Dispose()
            {
                if(_changed)
                {
                    GUI.backgroundColor = _oriColor;
                }
            }
        }

        // public static ObjectSelectWindow Instance { get; private set; }
        private string _search = "";
        private int _tabSelected;

        private Vector2 _scrollPos;
        private static float _scale;

        private static readonly Vector2 WidthScale = new Vector2(30f, 100f);

        protected class ItemInfo
        {
            // ReSharper disable InconsistentNaming
            public Object Object;
            public Texture2D Icon;
            // public bool HasInstanceId;
            public int InstanceID;
            public string Label;
            public GUIContent GuiLabel;
            // ReSharper enable InconsistentNaming
            public Texture2D preview;
            // public bool triedFirstLoad;
            public int failedCount;
        }

        private static readonly ItemInfo NullItemInfo = new ItemInfo
        {
            Icon = null,
            InstanceID = int.MinValue,
            Label = "None",
            Object = null,
            preview = null,
            GuiLabel = new GUIContent("None"),
            failedCount = int.MaxValue - 1,
        };

        // [MenuItem("Saints/Show")]
        // public static void TestShow()
        // {
        //     ObjectSelectWindow thisWindow = CreateInstance<ObjectSelectWindow>();
        //     // if (Instance == null)
        //     // {
        //     //     Instance = CreateInstance<ObjectSelectWindow>();
        //     // }
        //     // // Instance.ShowAuxWindow();
        //     thisWindow.Show();
        // }

        private IReadOnlyList<ItemInfo> _sceneItems;
        private IReadOnlyList<ItemInfo> _assetItems;
        private int _sceneItemSelectedIndex = -1;
        private int _assetItemSelectedIndex = -1;

        private string[] tabs;

        private bool _init;
        private Texture2D _closeIcon;
        // this is called before show in UI Toolkit env. Don't know why...
        // private void OnEnable()
        private void EnsureInit()
        {
            if (_init)
            {
                return;
            }

            _init = true;
            _closeIcon = Util.LoadResource<Texture2D>("classic-close.png");
            List<string> useTabs = new List<string>();
            // Debug.Log($"AllowAssets={AllowAssets}");
            if (AllowAssets)
            {
                useTabs.Add("Assets");
            }

            // Debug.Log($"AllowScene={AllowScene}");

            if (AllowScene)
            {
                useTabs.Add("Scene");
            }

            tabs = useTabs.ToArray();

            // Debug.Log($"OnEnable, _tabSelected={_tabSelected}, length={tabs.Length}");

            if(_tabSelected >= tabs.Length)
            {
                _tabSelected = 0;
            }
        }

        private void OnDestroy()
        {
            IEnumerable<ItemInfo> allItems = Array.Empty<ItemInfo>();
            if(_sceneItems != null)
            {
                allItems = allItems.Concat(_sceneItems);
            }
            if(_assetItems != null)
            {
                allItems = allItems.Concat(_assetItems);
            }

            foreach (ItemInfo itemInfo in allItems.Where(each => each.preview != null))
            {
                DestroyImmediate(itemInfo.preview);
            }
        }

        private GUIStyle _buttonStyle;

        protected void SetDefaultActive(Object target)
        {
            EnsureInit();

            if (target == null)
            {
                _assetItemSelectedIndex = _sceneItemSelectedIndex = 0;
                return;
            }

            if (AllowAssets)
            {
                foreach ((ItemInfo itemInfo, int index) in EnsureAssetItems().WithIndex().Skip(1))
                {
                    if (IsEqual(itemInfo, target))
                    {
                        _assetItemSelectedIndex = index;
                        _tabSelected = Array.IndexOf(tabs, "Assets");
                        _sceneItemSelectedIndex = -1;
                        return;
                    }
                }
            }

            if (AllowScene)
            {
                foreach ((ItemInfo itemInfo, int index) in EnsureSceneItems().WithIndex().Skip(1))
                {
                    if (IsEqual(itemInfo, target))
                    {
                        _sceneItemSelectedIndex = index;
                        _tabSelected = Array.IndexOf(tabs, "Scene");
                        _assetItemSelectedIndex = -1;
                        return;
                    }
                }
            }
        }

        // target is not null
        protected abstract bool IsEqual(ItemInfo itemInfo, Object target);

        private GUIStyle _labelCenterStyle;

        private void OnGUI()
        {
            EnsureInit();

            using (new EditorGUILayout.HorizontalScope())
            {
                const string controlName = "object-select-window-search";
                GUI.SetNextControlName(controlName);
                _search = EditorGUILayout.TextField(_search);

                if (!string.IsNullOrEmpty(_search) && GUILayout.Button(new GUIContent(_closeIcon), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
                {
                    // Debug.Log($"clicked");
                    _search = "";
                    GUI.FocusControl(controlName);  // this won't work with keyboardControl=0. The focus does NOT work here...
                    GUIUtility.keyboardControl = 0;
                    return;
                }
            }

            // Rect lastRect = GUILayoutUtility.GetLastRect();
            // Rect closeRect = new Rect(lastRect)
            // {
            //     x = lastRect.x + lastRect.width - 20,
            //     width = 20,
            // };
            // if (GUI.Button(closeRect, new GUIContent(_closeIcon), GUIStyle.none))
            // {
            //     Debug.Log($"clicked");
            // }

            Rect tabLine = EditorGUILayout.GetControlRect();
            // EditorGUI.DrawRect(tabLine, Color.black);
            Rect leftHalf = new Rect(tabLine)
            {
                width = 100,
            };
            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                using(new EditorGUI.DisabledScope(tabs.Length == 1))
                {
                    _tabSelected = GUI.Toolbar(leftHalf, _tabSelected, tabs);
                }

                if (changed.changed)
                {
                    _scrollPos = Vector2.zero;
                }
            }

            Event evt = Event.current;
            // Debug.Log(evt.type);
            if (evt.type == EventType.ScrollWheel)
            {
                if(Event.current.control || Event.current.command)
                {
                    // Debug.Log(evt.delta);
                    float vDelta = -evt.delta.y;
                    float normDelta = vDelta / 30f;
                    _scale = Mathf.Clamp(_scale + normDelta, 0f, 1f);
                    if(_scale < 0.2f)
                    {
                        _scale = vDelta > 0? 0.2f: 0f;
                    }
                }
            }

            bool isAssets = tabs[_tabSelected] == "Assets";
            // slider
            if (isAssets)
            {
                Rect sliderRect = new Rect(tabLine)
                {
                    x = tabLine.x + leftHalf.width + 10,
                    width = tabLine.width - leftHalf.width - 10,
                };
                // ReSharper disable once ConvertToUsingDeclaration
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float newValue = EditorGUI.Slider(sliderRect, _scale, 0f, 1f);
                    if (changed.changed)
                    {
                        if (newValue >= 0.2f)
                        {
                            _scale = newValue;
                        }
                        else
                        {
                            // float diff = newValue - _scale;
                            if (newValue >= 0.1f)
                            {
                                _scale = 0.2f;
                            }
                            else
                            {
                                _scale = 0f;
                            }
                        }

                    }
                }
                // EditorGUI.DrawRect(sliderRect, Color.yellow);
            }

            bool showPreviewImage = isAssets && _scale > 0.1f;

            int curSelectedIndex = isAssets ? _assetItemSelectedIndex : _sceneItemSelectedIndex;

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos,GUILayout.ExpandHeight(true)))
            {
                // scrollView.handleScrollWheel = !(Event.current.control || Event.current.command);
                _scrollPos = scrollView.scrollPosition;
                IEnumerable<ItemInfo> targets = isAssets
                    ? EnsureAssetItems()
                    : EnsureSceneItems();

                IEnumerable<(ItemInfo itemInfo, int index)> targetsWithIndex = targets.WithIndex();

                string[] searchParts = _search.Trim().Split();
                if(searchParts.Length > 0)
                {
                    targetsWithIndex = targetsWithIndex.Where(each => searchParts.All(part => each.itemInfo.Label.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (showPreviewImage)  // image view
                {
                    const float gap = 0.5f;
                    const float scrollerWidth = 18f;
                    float useWidth = EditorGUIUtility.currentViewWidth - gap * 2 - scrollerWidth;
                    float previewSize = Mathf.Lerp(WidthScale.x, WidthScale.y, Mathf.InverseLerp(0.2f, 1f, _scale));
                    int colCount;
                    if (previewSize >= useWidth)  // show one each col
                    {
                        colCount = 1;
                    }
                    else
                    {
                        colCount = Mathf.FloorToInt(useWidth / previewSize);
                    }

                    float colWidth = useWidth / colCount;

                    foreach (IReadOnlyList<(ItemInfo itemInfo, int index)> itemInfos in ChunkBy(targetsWithIndex, colCount))
                    {
                        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(colWidth));

                        foreach (((ItemInfo itemInfo, int itemIndex), int rowIndex) in itemInfos.WithIndex())
                        {
                            Rect thisRect = new Rect(rect)
                            {
                                width = previewSize,
                                height = previewSize,
                                x = rect.x + colWidth * rowIndex,
                            };

                            float labelHeight = EditorGUIUtility.singleLineHeight;
                            Rect previewRect = new Rect(thisRect)
                            {
                                height = thisRect.height - labelHeight - 4,
                                width = thisRect.height - labelHeight - 4,
                                x = thisRect.x + labelHeight / 2 + 2,
                                y = thisRect.y + 2,
                            };
                            Rect labelRect = new Rect(thisRect)
                            {
                                height = labelHeight,
                                y = thisRect.y + thisRect.height - labelHeight,
                            };

                            using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                            {
                                bool active = GUI.Toggle(thisRect, _assetItemSelectedIndex == itemIndex, "", GUI.skin.button);
                                if (changed.changed)
                                {
                                    OnItemClick(itemIndex, active, true);
                                    // _assetItemSelectedIndex = itemIndex;
                                }
                            }

                            float labelWidth = EditorStyles.label.CalcSize(itemInfo.GuiLabel).x;
                            GUIStyle labelStyle = EditorStyles.label;
                            if (labelWidth < labelRect.width)
                            {
                                if (_labelCenterStyle == null)
                                {
                                    _labelCenterStyle = new GUIStyle(EditorStyles.label)
                                    {
                                        alignment = TextAnchor.MiddleCenter,
                                    };
                                }
                                labelStyle = _labelCenterStyle;
                            }

                            EditorGUI.LabelField(labelRect, itemInfo.Label, labelStyle);
                            Texture2D previewTexture = GetPreview(itemInfo);
                            // let's bypass Unity's life cycle null check
                            if(!previewTexture)
                            {
                                previewTexture = itemInfo.Icon;
                            }

                            if (previewTexture)
                            {
                                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
                            }
                            // {
                            //     GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
                            // }
                            // else if (itemInfo.Icon)
                            // {
                            //     GUI.DrawTexture(previewRect, itemInfo.Icon, ScaleMode.ScaleToFit);
                            // }
                        }
                        // EditorGUILayout.EndHorizontal();
                    }
                }
                else  // list view
                {
                    // GUISkin skin = Util.LoadResource<GUISkin>("IMGUI/TButton.guiskin");
                    if(_buttonStyle == null)
                    {
                        _buttonStyle = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            // border = new RectOffset(0, 0, 0, 0),
                            border = GUI.skin.label.border,
                            margin = new RectOffset(0, 0, 0, 0),
                            overflow = new RectOffset(0, 0, 0, 0),
                            // normal =
                            // {
                            //     background = Texture2D.blackTexture,
                            // }
                            // padding = new RectOffset(0, 0, 0, 0),
                            // padding = 0,
                        };
                    }

                    foreach ((ItemInfo itemInfo, int itemIndex) in targetsWithIndex)
                    {
                        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUI.skin.label);
                        rect.height += 3;
                        rect.y -= 2;
                        // EditorGUI.DrawRect(rect, Color.blue);

                        bool mouseOver = mouseOverWindow == this && rect.Contains(evt.mousePosition);
                        bool selected = curSelectedIndex == itemIndex;

                        // GUI.backgroundColor = Color.magenta;
                        using(new GUIBackgroundScoop(!mouseOver && !selected, Color.clear))
                        using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                        {
                            bool active = GUI.Toggle(rect, selected, new GUIContent(itemInfo.Label, itemInfo.Icon), _buttonStyle);
                            // ReSharper disable once InvertIf
                            if (changed.changed)
                            {
                                OnItemClick(itemIndex, active, isAssets);
                            }
                        }
                        // GUI.Button(rect, new GUIContent(itemInfo.Label, itemInfo.Icon), _buttonStyle);
                        // if()
                        // {
                        //     if (isAssets)
                        //     {
                        //         _assetItemSelectedIndex = itemIndex;
                        //     }
                        //     else
                        //     {
                        //         _sceneItemSelectedIndex = itemIndex;
                        //     }
                        // }
                    }
                }

            }

            if (Error != "")
            {
                EditorGUILayout.HelpBox(Error, MessageType.Error);
            }

            if(curSelectedIndex > 0)  // selected not null
            {
                Rect selectRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));

                EditorGUI.DrawRect(selectRect, EColor.EditorEmphasized.GetColor());

                Rect selectPreviewRect = new Rect(selectRect.x + 5, selectRect.y + 5, 70, 70);
                ItemInfo itemInfo = isAssets ? _assetItems[curSelectedIndex] : _sceneItems[curSelectedIndex];
                if(isAssets)
                {
                    itemInfo.failedCount = 0;
                    Texture2D previewTexture = GetPreview(itemInfo);
                    // let's bypass Unity's life cycle null check
                    if (!previewTexture)
                    {
                        previewTexture = itemInfo.Icon;
                    }

                    if (previewTexture)
                    {
                        GUI.DrawTexture(selectPreviewRect, previewTexture, ScaleMode.ScaleToFit);
                    }
                }

                Rect selectInfoRect = new Rect(selectRect.x + 80, selectRect.y + 5, selectRect.width - 80, 70);
                Rect selectNameRect = new Rect(selectInfoRect)
                {
                    height = EditorGUIUtility.singleLineHeight,
                };
                EditorGUI.LabelField(selectNameRect, itemInfo.Label);
                Rect selectTypeRect = new Rect(selectInfoRect)
                {
                    y = selectNameRect.y + selectNameRect.height,
                    height = EditorGUIUtility.singleLineHeight,
                };
                EditorGUI.LabelField(selectTypeRect, itemInfo.Object?.GetType().Name ?? "");
            }


            // EditorGUI.DrawRect(searchRect, Color.blue);
        }

        private double lastActiveClickTime = double.MinValue;

        private void OnItemClick(int itemIndex, bool active, bool isAssets)
        {
            bool isClickActive = !active;
            double curTime = EditorApplication.timeSinceStartup;
            var itemInfo = isAssets ? _assetItems[itemIndex] : _sceneItems[itemIndex];
            OnSelect(itemInfo);
            if (isClickActive)
            {
                if (curTime - lastActiveClickTime < 0.5f)
                {
                    // double click
                    // Debug.Log($"Double Click {itemIndex}");
                    Close();
                    return;
                }

                lastActiveClickTime = curTime;
                return;
            }

            lastActiveClickTime = curTime;

            if (isAssets)
            {
                _assetItemSelectedIndex = itemIndex;
            }
            else
            {
                _sceneItemSelectedIndex = itemIndex;
            }
        }

        private IReadOnlyList<ItemInfo> EnsureAssetItems()
        {
            if(_assetItems == null)
            {
                _assetItems = FetchAllAssets().Where(FetchAllAssetsFilter).Prepend(NullItemInfo).ToArray();
            }

            return _assetItems;
        }

        protected virtual IEnumerable<ItemInfo> FetchAllAssets()
        {
            return HelperFetchAllAssets();
        }

        private IReadOnlyList<ItemInfo> EnsureSceneItems()
        {
            if (_sceneItems == null)
            {
                _sceneItems = FetchAllSceneObject().Where(FetchAllSceneObjectFilter).Prepend(NullItemInfo).ToArray();
            }

            return _sceneItems;
        }

        protected virtual IEnumerable<ItemInfo> FetchAllSceneObject()
        {
            return HelperFetchAllSceneObject();
        }

        protected abstract void OnSelect(ItemInfo itemInfo);

        private static Texture2D GetPreview(ItemInfo itemInfo)
        {
            if (!itemInfo.Object)
            {
                return null;
            }

            // ReSharper disable once Unity.NoNullPropagation
            if (itemInfo.preview && itemInfo.preview.width != 1)
            {
                // Debug.Log($"return preview {itemInfo.preview.width}");
                return itemInfo.preview;
            }

            if(AssetPreview.IsLoadingAssetPreview(itemInfo.InstanceID))
            {
                // Debug.Log($"loading preview {itemInfo.Label}");
                return null;
            }

            if (itemInfo.failedCount > 5)
            {
                return null;
            }

            // itemInfo.triedFirstLoad = true;

            // Texture2D result = AssetPreview.GetAssetPreview(itemInfo.Object);
            Texture2D result;
            try
            {
                result = AssetPreview.GetAssetPreview(itemInfo.Object);
            }
            catch (AssertionException)  // Unity: Assertion failed on expression: 'i->previewArtifactID == found->second.previewArtifactID'
            {
                return null;
            }
            // Debug.Log($"return preview {result?.width}");
            if (result && result.width != 1)
            {
                itemInfo.preview = result;
                return result;
            }

            itemInfo.failedCount += 1;

            return null;
        }

        private static IEnumerable<IReadOnlyList<T>> ChunkBy<T>(IEnumerable<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList());
        }

        private static IEnumerable<ItemInfo> HelperFetchAllSceneObject()
        {
            HierarchyProperty property = new HierarchyProperty(HierarchyType.GameObjects, false);

            while (property.Next(null))
            {
                GameObject go = property.pptrValue as GameObject;
                if (go == null)
                {
                    continue;
                }
                yield return new ItemInfo { Object = go, Icon = property.icon, InstanceID = property.instanceID, Label = property.name, GuiLabel = new GUIContent(property.name)};
            }
        }

        private static IEnumerable<ItemInfo> HelperFetchAllAssets()
        {
            HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);

            // property.SetSearchFilter(search, 0);

            while (property.Next(null))
            {
                Object go = property.pptrValue;
                if (go == null)
                {
                    // go = null;  // Object(null) is not null in Unity because Unity overrides `==`
                    continue;
                }

                yield return new ItemInfo { Object = go, Icon = property.icon, InstanceID = property.instanceID, Label = property.name, GuiLabel = new GUIContent(property.name)};
            }
        }

        // protected bool PreFilter()

        protected abstract bool FetchAllSceneObjectFilter(ItemInfo itemInfo);
        protected abstract bool FetchAllAssetsFilter(ItemInfo itemInfo);
    }
}
