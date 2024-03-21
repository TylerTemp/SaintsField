using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEngine;
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
        private float _scale;

        private static readonly Vector2 WidthScale = new Vector2(30f, 100f);

        public class ItemInfo
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
        private int _sceneItemSelectedIndex;
        private int _assetItemSelectedIndex;

        private string[] tabs;

        private void OnEnable()
        {
            List<string> useTabs = new List<string>();
            if (AllowAssets)
            {
                useTabs.Add("Assets");
            }

            if (AllowScene)
            {
                useTabs.Add("Scene");
            }

            tabs = useTabs.ToArray();

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

        private void OnGUI()
        {
            _search = EditorGUILayout.TextField(_search);

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
                IEnumerable<ItemInfo> targets;
                if (isAssets)
                {
                    if(_assetItems == null)
                    {
                        _assetItems = FetchAllAssets().Prepend(NullItemInfo).ToArray();
                    }

                    targets = _assetItems;
                }
                else
                {
                    if (_sceneItems == null)
                    {
                        _sceneItems = FetchAllSceneObject().Prepend(NullItemInfo).ToArray();
                    }
                    targets = _sceneItems;
                }

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

                            EditorGUI.LabelField(labelRect, itemInfo.Label);
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
                Rect searchRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));
                Rect selectPreviewRect = new Rect(searchRect.x + 5, searchRect.y + 5, 70, 70);
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

                Rect selectInfoRect = new Rect(searchRect.x + 80, searchRect.y + 5, searchRect.width - 80, 70);
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

            Texture2D result = AssetPreview.GetAssetPreview(itemInfo.Object);
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

        private static IEnumerable<ItemInfo> FetchAllSceneObject()
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

        private static IEnumerable<ItemInfo> FetchAllAssets()
        {
            HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);

            // property.SetSearchFilter(search, 0);

            while (property.Next(null))
            {
                Object go = property.pptrValue;
                if (go == null)
                {
                    go = null;  // Object(null) is not null in Unity because Unity overrides `==`
                }

                yield return new ItemInfo { Object = go, Icon = property.icon, InstanceID = property.instanceID, Label = property.name, GuiLabel = new GUIContent(property.name)};
            }
        }

        // protected bool PreFilter()
    }
}
