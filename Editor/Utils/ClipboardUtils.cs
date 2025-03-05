// using System;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// #if SAINTSFIELD_NEWTONSOFT_JSON
// using Newtonsoft.Json;
// using UnityEditor;
// #endif
//
// namespace SaintsField.Editor.Utils
// {
//     public static class ClipboardUtils
//     {
//         [Serializable]
//         public enum ObjectWrapperType
//         {
//             GeneralClassStruct = -1,
//             InSceneGameObjectComponent = 0,
//             ScriptableObject = 2,
//             GameObjectComponentPrefab = 3,
//             Color = 4,
//             Vector2 = 8,
//             AnimationCurve = 14,
//         }
//
//         [Serializable]
//         public struct ObjectWrapperJSON
//         {
//             public string guid;
//             public long localId;
//             public ObjectWrapperType type;
//             public int instanceID;
//         }
//
//         [Serializable]
//         public class AnimationCurveWrapperJSON
//         {
//             public AnimationCurve curve;
//         }
//
//         public static string CopyUnityObject(UnityEngine.Object uObject)
//         {
//
//
//             return $"UnityEditor.ObjectWrapperJSON:";
//         }
//
//         public static string CopyGenericString(object norObject)
//         {
//             if (norObject is AnimationCurve ac)
//             {
//                 return CopyAnimatorCurveToString(ac);
//             }
// #if SAINTSFIELD_NEWTONSOFT_JSON
//             string genJson;
//             try
//             {
//                 genJson = JsonConvert.SerializeObject(norObject);
//             }
//             catch (Exception e)
//             {
// #if SAINTSFIELD_DEBUG
//                 Debug.LogException(e);
// #endif
//                 return "";
//             }
//             return $"GenericPropertyJSON:{genJson}";
// #endif
//
//             return "";
//         }
//
//         private static string CopyAnimatorCurveToString(AnimationCurve ac)
//         {
// #if SAINTSFIELD_NEWTONSOFT_JSON
//             return $"UnityEditor.AnimationCurveWrapperJSON:{JsonConvert.SerializeObject(ac)}";
// #endif
//
// #pragma warning disable CS0162 // Unreachable code detected
//             return "";
// #pragma warning restore CS0162 // Unreachable code detected
//         }
//
//         private static string UnityObjectToJson(UnityEngine.Object uObject)
//         {
// #if SAINTSFIELD_NEWTONSOFT_JSON
//             if (uObject is GameObject go)
//             {
//                 return UnityObjectToJsonWithScene(go, go.scene.IsValid()? ObjectWrapperType.InSceneGameObjectComponent: ObjectWrapperType.GameObjectComponentPrefab);
//             }
//
//             if (uObject is Component comp)
//             {
//                 return UnityObjectToJsonWithScene(comp, comp.gameObject.scene.IsValid()? ObjectWrapperType.InSceneGameObjectComponent: ObjectWrapperType.GameObjectComponentPrefab);
//             }
//
//             if (uObject is ScriptableObject so)
//             {
//                 return UnityObjectToJsonWithScene(so, ObjectWrapperType.ScriptableObject);
//             }
// #endif
//             return "";
//         }
//
//         private static string UnityObjectToJsonWithScene(UnityEngine.Object uObject, ObjectWrapperType objectWrapperType)
//         {
// #if SAINTSFIELD_NEWTONSOFT_JSON
//
//             AssetDatabase.TryGetGUIDAndLocalFileIdentifier(uObject, out string guid, out long localId);
//
//             return JsonConvert.SerializeObject(new ObjectWrapperJSON
//             {
//                 guid = guid ?? "",
//                 localId = localId,
//                 type = objectWrapperType,
//                 instanceID = uObject.GetInstanceID(),
//             });
// #endif
//
// #pragma warning disable CS0162 // Unreachable code detected
//             // ReSharper disable once HeuristicUnreachableCode
//             return "";
// #pragma warning restore CS0162 // Unreachable code detected
//         }
//     }
// }
