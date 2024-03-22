// using UnityEditor;
// using UnityEngine;
//
// namespace SaintsField.Editor.Utils
// {
//     public class ObjectSelectWindowTest: ObjectSelectWindow
//     {
//         [MenuItem("Saints/Show")]
//         public static void TestShow()
//         {
//             ObjectSelectWindow thisWindow = CreateInstance<ObjectSelectWindowTest>();
//             thisWindow.titleContent = new GUIContent("Object Select Window Test");
//             // if (Instance == null)
//             // {
//             //     Instance = CreateInstance<ObjectSelectWindow>();
//             // }
//             // // Instance.ShowAuxWindow();
//             thisWindow.Show();
//         }
//
//         protected override bool AllowScene => true;
//         protected override bool AllowAssets => true;
//         protected override string Error => "";
//
//         protected override void OnSelect(ItemInfo itemInfo)
//         {
//             Debug.Log($"{itemInfo.Label}: {itemInfo.Object}");
//         }
//
//         protected override bool FetchAllSceneObjectFilter(ItemInfo itemInfo) => true;
//         protected override bool FetchAllAssetsFilter(ItemInfo itemInfo) => true;
//     }
// }
