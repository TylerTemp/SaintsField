using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils
{
    public class ObjectSelectWindow: EditorWindow
    {
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

        private class ItemInfo
        {
            // ReSharper disable InconsistentNaming
            public Object Object;
            public Texture2D Icon;
            // public bool HasInstanceId;
            public int InstanceID;
            public string Label;
            // ReSharper enable InconsistentNaming
            public Texture2D preview;
        }

        private static readonly ItemInfo NullItemInfo = new ItemInfo
        {
            Icon = null,
            InstanceID = int.MinValue,
            Label = "None",
            Object = null,
            preview = null,
        };

        [MenuItem("Saints/Show")]
        public static void TestShow()
        {
            ObjectSelectWindow thisWindow = CreateInstance<ObjectSelectWindow>();
            // if (Instance == null)
            // {
            //     Instance = CreateInstance<ObjectSelectWindow>();
            // }
            // // Instance.ShowAuxWindow();
            thisWindow.Show();
        }

        private IReadOnlyList<ItemInfo> _sceneItems;
        private IReadOnlyList<ItemInfo> _assetItems;
        private int _sceneItemSelectedIndex;
        private int _assetItemSelectedIndex;

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

        private GUIStyle _buttonStyle = null;

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
                _tabSelected = GUI.Toolbar(leftHalf, _tabSelected, new[]
                {
                    "Assets",
                    "Scene",
                });

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
            bool isAssets = _tabSelected == 0;
            // slider
            if (isAssets)
            {
                Rect sliderRect = new Rect(tabLine)
                {
                    x = tabLine.x + leftHalf.width + 10,
                    width = tabLine.width - leftHalf.width - 10,
                };
                // ReSharper disable once ConvertToUsingDeclaration
                using (var changed = new EditorGUI.ChangeCheckScope())
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
                        _assetItems = FetchAllAssets("").Prepend(NullItemInfo).ToArray();
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

                string[] searchParts = _search.Trim().Split();
                if(searchParts.Length > 0)
                {
                    targets = targets.Where(each => searchParts.All(part => each.Label.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0));
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

                    // Debug.Log($"EditorGUIUtility.currentViewWidth={EditorGUIUtility.currentViewWidth}, previewSize={previewSize}, count={colCount}");

                    // Debug.Log($"previewSize={previewSize}, colCount={colCount}");

                    float colWidth = useWidth / colCount;

                    foreach (IReadOnlyList<(ItemInfo itemInfo, int index)> itemInfos in ChunkBy(targets.WithIndex(), colCount))
                    {
                        // EditorGUILayout.BeginHorizontal();
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
                                bool selected = GUI.Toggle(thisRect, _assetItemSelectedIndex == itemIndex, "", "button");
                                if (changed.changed && selected)
                                {
                                    _assetItemSelectedIndex = itemIndex;
                                }
                            }

                            EditorGUI.LabelField(labelRect, itemInfo.Label);
                            Texture2D previewTexture = GetPreview(itemInfo);
                            // let's bypass Unity's life cycle null check
                            if(previewTexture)
                            {
                                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
                            }
                            else if (itemInfo.Icon)
                            {
                                GUI.DrawTexture(previewRect, itemInfo.Icon, ScaleMode.ScaleToFit);
                                // EditorGUI.LabelField(previewRect, "I");
                            }

                            // buttonStyleToggled.active.background = activeButtonColor;

                            // if (itemInfo.Icon != null)
                            // {
                            //     EditorGUI.DrawPreviewTexture(thisRect, itemInfo.Icon);
                            // }
                            // else
                            // {
                            //     EditorGUI.DrawRect(thisRect, Color.black);
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

                    foreach ((ItemInfo itemInfo, int itemIndex) in targets.WithIndex())
                    {
                        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUI.skin.label);
                        rect.height += 4;
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
                            if (changed.changed && active)
                            {
                                if(isAssets)
                                {
                                    _assetItemSelectedIndex = itemIndex;
                                }
                                else
                                {
                                    _sceneItemSelectedIndex = itemIndex;
                                }
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

            if(curSelectedIndex > 0)  // selected not null
            {
                Rect searchRect = EditorGUILayout.GetControlRect(GUILayout.Height(80));
                Rect selectPreviewRect = new Rect(searchRect.x + 5, searchRect.y + 5, 70, 70);
                var itemInfo = isAssets ? _assetItems[curSelectedIndex] : _sceneItems[curSelectedIndex];
                Texture2D previewTexture = GetPreview(itemInfo);
                // let's bypass Unity's life cycle null check
                if(!previewTexture)
                {
                    previewTexture = itemInfo.Icon;
                }

                if (previewTexture)
                {
                    GUI.DrawTexture(selectPreviewRect, previewTexture, ScaleMode.ScaleToFit);
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

        private static Texture2D GetPreview(ItemInfo itemInfo)
        {
            if (!itemInfo.Object)
            {
                return null;
            }

            if (itemInfo.preview != null && itemInfo.preview.width != 1)
            {
                return itemInfo.preview;
            }

            if(AssetPreview.IsLoadingAssetPreview(itemInfo.InstanceID))
            {
                return null;
            }

            return itemInfo.preview = AssetPreview.GetAssetPreview(itemInfo.Object);
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
                yield return new ItemInfo { Object = go, Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
            }
        }

        private static IEnumerable<ItemInfo> FetchAllAssets(string search)
        {
            HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);
            property.SetSearchFilter(search, 0);

            while (property.Next(null))
            {
                Object go = property.pptrValue;
                if (go == null)
                {
                    go = null;  // Object(null) is not null in Unity because Unity overrides `==`
                }

                yield return new ItemInfo { Object = go, Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
            }
        }

        // protected bool PreFilter()
    }
}
