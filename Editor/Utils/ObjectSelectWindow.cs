// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
//
// namespace SaintsField.Editor.Utils
// {
//     public class ObjectSelectWindow: EditorWindow
//     {
//         public static ObjectSelectWindow Instance { get; private set; }
//         private string _search;
//         private int _tabSelected;
//
//         private Vector2 _scrollPos;
//
//         private struct ItemInfo
//         {
//             // ReSharper disable InconsistentNaming
//             public Texture Icon;
//             public bool hasInstanceId;
//             public int InstanceID;
//             public string Label;
//             // ReSharper enable InconsistentNaming
//         }
//
//         [MenuItem("Saints/Show")]
//         public static void TestShow()
//         {
//             if (Instance == null)
//             {
//                 Instance = CreateInstance<ObjectSelectWindow>();
//             }
//             // Instance.ShowAuxWindow();
//             Instance.Show();
//         }
//
//         private IReadOnlyCollection<ItemInfo> _sceneItems;
//         private IReadOnlyCollection<ItemInfo> _assetItems;
//
//         private void OnGUI()
//         {
//             float height = position.height;
//
//             _search = EditorGUILayout.TextField(_search);
//             height -= GUILayoutUtility.GetLastRect().height;
//             //
//             // Rect tabLine = EditorGUILayout.GetControlRect();
//             // height -= tabLine.height;
//             // // EditorGUI.DrawRect(tabLine, Color.black);
//             // Rect leftHalf = new Rect(tabLine)
//             // {
//             //     width = 100,
//             // };
//             // _tabSelected = GUI.Toolbar(leftHalf, _tabSelected, new[]
//             // {
//             //     "Assets",
//             //     "Scene",
//             // });
//
//             // Rect leftRect = new Rect(tabLine)
//             // {
//             //     y = tabLine.y + tabLine.height,
//             //     height = height - 10,
//             // };
//
//             // Debug.Log(leftRect);
//             // EditorGUI.DrawRect(leftRect, Color.blue);
//
//             const float previewArea = 1f;
//
//             float scrollViewHeight = height - previewArea;
//
//             height = previewArea;
//
//             // Rect scrollRect = new Rect(0, position.height - height, position.width, height - previewArea);
//             using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos, GUILayout.Height(scrollViewHeight)))
//             {
//                 _scrollPos = scrollView.scrollPosition;
//                 bool isAssets = _tabSelected == 0;
//                 IEnumerable<ItemInfo> targets;
//                 if (isAssets)
//                 {
//                     if(_assetItems == null)
//                     {
//                         _assetItems = FetchAllAssets(_search).ToArray();
//                     }
//
//                     targets = _assetItems;
//                 }
//                 else
//                 {
//                     if (_sceneItems == null)
//                     {
//                         _sceneItems = FetchAllSceneObject().ToArray();
//                     }
//                     targets = _sceneItems;
//                 }
//
//                 foreach (ItemInfo sceneItem in targets)
//                 {
//                     EditorGUILayout.LabelField(sceneItem.Label);
//                 }
//             }
//             //
//             // Rect scrollRect = GUILayoutUtility.GetLastRect();
//             // Rect lastRect = new Rect(scrollRect)
//             // {
//             //     y = scrollRect.y + scrollRect.height,
//             //     height = height,
//             // };
//             //
//             // EditorGUI.DrawRect(lastRect, Color.red);
//         }
//
//         private static IEnumerable<ItemInfo> FetchAllSceneObject()
//         {
//             HierarchyProperty property = new HierarchyProperty(HierarchyType.GameObjects, false);
//
//             while (property.Next(null))
//             {
//                 GameObject go = property.pptrValue as GameObject;
//                 if (go == null) continue;
//                 yield return new ItemInfo { Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
//             }
//         }
//
//         private IEnumerable<ItemInfo> FetchAllAssets(string search)
//         {
//             var property = new HierarchyProperty(HierarchyType.Assets, false);
//             property.SetSearchFilter(search, 0);
//
//             while (property.Next(null))
//             {
//                 yield return new ItemInfo { Icon = property.icon, InstanceID = property.instanceID, Label = property.name };
//             }
//         }
//     }
// }
