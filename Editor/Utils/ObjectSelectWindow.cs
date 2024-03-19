using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Linq;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils
{
    public class ObjectSelectWindow: EditorWindow
    {
        public static ObjectSelectWindow Instance { get; private set; }
        private string _search;
        private int _tabSelected;

        private Vector2 _scrollPos;
        private float _scale;

        private static readonly Vector2 WidthScale = new Vector2(30f, 100f);

        private struct ItemInfo
        {
            // ReSharper disable InconsistentNaming
            public Texture Icon;
            public bool hasInstanceId;
            public int InstanceID;
            public string Label;
            // ReSharper enable InconsistentNaming
        }

        [MenuItem("Saints/Show")]
        public static void TestShow()
        {
            if (Instance == null)
            {
                Instance = CreateInstance<ObjectSelectWindow>();
            }
            // Instance.ShowAuxWindow();
            Instance.Show();
        }

        private IReadOnlyList<ItemInfo> _sceneItems;
        private IReadOnlyList<ItemInfo> _assetItems;

        private void OnGUI()
        {
            EditorGUILayout.TextField(_search);

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

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos,GUILayout.ExpandHeight(true)))
            {
                _scrollPos = scrollView.scrollPosition;
                IReadOnlyList<ItemInfo> targets;
                if (isAssets)
                {
                    if(_assetItems == null)
                    {
                        _assetItems = FetchAllAssets(_search).ToArray();
                    }

                    targets = _assetItems;
                }
                else
                {
                    if (_sceneItems == null)
                    {
                        _sceneItems = FetchAllSceneObject().ToArray();
                    }
                    targets = _sceneItems;
                }

                // foreach (ItemInfo sceneItem in targets)
                // {
                //     EditorGUILayout.LabelField(sceneItem.Label);
                // }

                if (showPreviewImage)
                {
                    const float gap = 0.5f;
                    float useWidth = EditorGUIUtility.currentViewWidth - gap * 2;
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

                    // Debug.Log($"previewSize={previewSize}, colCount={colCount}");

                    float colWidth = useWidth / colCount;

                    foreach (IReadOnlyList<ItemInfo> itemInfos in ChunkBy(targets, colCount))
                    {
                        // EditorGUILayout.BeginHorizontal();
                        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(colWidth));

                        foreach ((ItemInfo itemInfo, int index) in itemInfos.WithIndex())
                        {
                            Rect thisRect = new Rect(rect)
                            {
                                width = previewSize,
                                height = previewSize,
                                x = rect.x + colWidth * index,
                            };


                            GUIStyle style = new GUIStyle("button")
                            {
                            };

                            GUI.Toggle(thisRect, Random.Range(0, 2) == 1, "", style);

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

            }

            Rect searchRect = EditorGUILayout.GetControlRect(GUILayout.Height(30));

            EditorGUI.DrawRect(searchRect, Color.blue);
        }

        public static IEnumerable<IReadOnlyList<T>> ChunkBy<T>(IEnumerable<T> source, int chunkSize)
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
                yield return new ItemInfo { Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
            }
        }

        private IEnumerable<ItemInfo> FetchAllAssets(string search)
        {
            HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);
            property.SetSearchFilter(search, 0);

            while (property.Next(null))
            {
                yield return new ItemInfo { Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
            }
        }
    }
}
