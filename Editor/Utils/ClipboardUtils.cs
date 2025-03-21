using System;
using UnityEngine;
using UnityEditor;
#if SAINTSFIELD_NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace SaintsField.Editor.Utils
{
    public static class ClipboardUtils
    {
        [Serializable]
        public enum ObjectWrapperType
        {
            GeneralClassStruct = -1,
            InSceneGameObjectComponent = 0,
            ScriptableObject = 2,
            GameObjectComponentPrefab = 3,
            Color = 4,
            Vector2 = 8,
            AnimationCurve = 14,
        }

        [Serializable]
        // ReSharper disable once InconsistentNaming
        public struct ObjectWrapperJSON
        {
            public string guid;
            public long localId;
            public ObjectWrapperType type;
            public int instanceID;
        }

        [Serializable]
        // ReSharper disable once InconsistentNaming
        public class AnimationCurveWrapperJSON
        {
            public AnimationCurve curve;
        }

        public static string CopyUnityObject(UnityEngine.Object uObject)
        {
            return $"UnityEditor.ObjectWrapperJSON:";
        }

        public static string CopyGenericType(object norObject)
        {
#if SAINTSFIELD_NEWTONSOFT_JSON
            if (norObject is bool boolV)
            {
                return JsonConvert.SerializeObject(boolV);
            }
            if (norObject is sbyte sByteV)
            {
                return JsonConvert.SerializeObject(sByteV);
            }
            if (norObject is byte byteV)
            {
                return JsonConvert.SerializeObject(byteV);
            }
            if(norObject is short shortV)
            {
                return JsonConvert.SerializeObject(shortV);
            }
            if (norObject is ushort ushortV)
            {
                return JsonConvert.SerializeObject(ushortV);
            }
            if (norObject is int intV)
            {
                return JsonConvert.SerializeObject(intV);
            }
            if (norObject is uint uintV)
            {
                return JsonConvert.SerializeObject(uintV);
            }
            if (norObject is long longV)
            {
                return JsonConvert.SerializeObject(longV);
            }
            if (norObject is ulong ulongV)
            {
                return JsonConvert.SerializeObject(ulongV);
            }
            if (norObject is float floatV)
            {
                return JsonConvert.SerializeObject(floatV);
            }
            if (norObject is double doubleV)
            {
                return JsonConvert.SerializeObject(doubleV);
            }
            if (norObject is string stringV)
            {
                return stringV;
            }

            if (norObject is AnimationCurve ac)
            {
                return CopyAnimatorCurveToString(ac);
            }

            string genJson;
            try
            {
                genJson = JsonConvert.SerializeObject(norObject);
            }
            catch (Exception e)
            {
#if SAINTSFIELD_DEBUG
                Debug.LogException(e);
#endif
                return "";
            }
            return $"GenericPropertyJSON:{genJson}";
#else
            return "";
#endif
        }

        private static string CopyAnimatorCurveToString(AnimationCurve ac)
        {
#if SAINTSFIELD_NEWTONSOFT_JSON
            return $"UnityEditor.AnimationCurveWrapperJSON:{JsonConvert.SerializeObject(ac)}";
#endif

#pragma warning disable CS0162 // Unreachable code detected
            return "";
#pragma warning restore CS0162 // Unreachable code detected
        }

        private static string UnityObjectToJson(UnityEngine.Object uObject)
        {
#if SAINTSFIELD_NEWTONSOFT_JSON
            if (uObject is GameObject go)
            {
                return UnityObjectToJsonWithScene(go, go.scene.IsValid()? ObjectWrapperType.InSceneGameObjectComponent: ObjectWrapperType.GameObjectComponentPrefab);
            }

            if (uObject is Component comp)
            {
                return UnityObjectToJsonWithScene(comp, comp.gameObject.scene.IsValid()? ObjectWrapperType.InSceneGameObjectComponent: ObjectWrapperType.GameObjectComponentPrefab);
            }

            if (uObject is ScriptableObject so)
            {
                return UnityObjectToJsonWithScene(so, ObjectWrapperType.ScriptableObject);
            }
#endif
            return "";
        }

        private static string UnityObjectToJsonWithScene(UnityEngine.Object uObject, ObjectWrapperType objectWrapperType)
        {
#if SAINTSFIELD_NEWTONSOFT_JSON

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(uObject, out string guid, out long localId);

            return JsonConvert.SerializeObject(new ObjectWrapperJSON
            {
                guid = guid ?? "",
                localId = localId,
                type = objectWrapperType,
                instanceID = uObject.GetInstanceID(),
            });
#endif

#pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return "";
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
