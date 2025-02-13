using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Core
{
    public class SaintsAssetPostprocessor: AssetPostprocessor
    {
        public static readonly UnityEvent OnAnyEvent = new UnityEvent();

        public class OnAssignMaterialModelEventClass: UnityEvent<Material, Renderer>{}
        public static readonly OnAssignMaterialModelEventClass OnAssignMaterialModelEvent = new OnAssignMaterialModelEventClass();
        private Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            OnAssignMaterialModelEvent.Invoke(material, renderer);
            OnAnyEvent.Invoke();
            return null;
        }

        public class OnPostprocessAllAssetsEventClass: UnityEvent<string[], string[], string[], string[]>{}
        public static readonly OnPostprocessAllAssetsEventClass OnPostprocessAllAssetsEvent = new OnPostprocessAllAssetsEventClass();
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            OnPostprocessAllAssetsEvent.Invoke(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessAnimationEventClass: UnityEvent<GameObject, AnimationClip>{}
        public static readonly OnPostprocessAnimationEventClass OnPostprocessAnimationEvent = new OnPostprocessAnimationEventClass();
        private void OnPostprocessAnimation(GameObject go, AnimationClip ac)
        {
            OnPostprocessAnimationEvent.Invoke(go, ac);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessAssetbundleNameChangedEventClass: UnityEvent<string, string, string>{}
        public static readonly OnPostprocessAssetbundleNameChangedEventClass OnPostprocessAssetbundleNameChangedEvent = new OnPostprocessAssetbundleNameChangedEventClass();
        private void OnPostprocessAssetbundleNameChanged(string a, string b, string c)
        {
            OnPostprocessAssetbundleNameChangedEvent.Invoke(a, b, c);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessAudioEventClass: UnityEvent<AudioClip>{}
        public static readonly OnPostprocessAudioEventClass OnPostprocessAudioEvent = new OnPostprocessAudioEventClass();
        private void OnPostprocessAudio(AudioClip ac)
        {
            OnPostprocessAudioEvent.Invoke(ac);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessCubemapEventClass: UnityEvent<Cubemap>{}
        public static readonly OnPostprocessCubemapEventClass OnPostprocessCubemapEvent = new OnPostprocessCubemapEventClass();
        private void OnPostprocessCubemap(Cubemap cm)
        {
            OnPostprocessCubemapEvent.Invoke(cm);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessGameObjectWithAnimatedUserPropertiesEventClass: UnityEvent<GameObject, EditorCurveBinding[]>{}
        public static readonly OnPostprocessGameObjectWithAnimatedUserPropertiesEventClass OnPostprocessGameObjectWithAnimatedUserPropertiesEvent = new OnPostprocessGameObjectWithAnimatedUserPropertiesEventClass();
        private void OnPostprocessGameObjectWithAnimatedUserProperties(GameObject go, EditorCurveBinding[] bindings)
        {
            OnPostprocessGameObjectWithAnimatedUserPropertiesEvent.Invoke(go, bindings);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessGameObjectWithUserPropertiesEventClass: UnityEvent<GameObject, string[], object[]>{}
        public static readonly OnPostprocessGameObjectWithUserPropertiesEventClass OnPostprocessGameObjectWithUserPropertiesEvent = new OnPostprocessGameObjectWithUserPropertiesEventClass();
        private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propertyNames, object[] values)
        {
            OnPostprocessGameObjectWithUserPropertiesEvent.Invoke(go, propertyNames, values);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessMaterialEventClass: UnityEvent<Material>{}
        public static readonly OnPostprocessMaterialEventClass OnPostprocessMaterialEvent = new OnPostprocessMaterialEventClass();
        private void OnPostprocessMaterial(Material material)
        {
            OnPostprocessMaterialEvent.Invoke(material);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessMeshHierarchyEventClass: UnityEvent<GameObject>{}
        public static readonly OnPostprocessMeshHierarchyEventClass OnPostprocessMeshHierarchyEvent = new OnPostprocessMeshHierarchyEventClass();
        private void OnPostprocessMeshHierarchy(GameObject go)
        {
            OnPostprocessMeshHierarchyEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessModelEventClass: UnityEvent<GameObject>{}
        public static readonly OnPostprocessModelEventClass OnPostprocessModelEvent = new OnPostprocessModelEventClass();
        private void OnPostprocessModel(GameObject go)
        {
            OnPostprocessModelEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessPrefabEventClass: UnityEvent<GameObject>{}
        public static readonly OnPostprocessPrefabEventClass OnPostprocessPrefabEvent = new OnPostprocessPrefabEventClass();
        private void OnPostprocessPrefab(GameObject go)
        {
            OnPostprocessPrefabEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessSpeedTreeEventClass: UnityEvent<GameObject>{}
        public static readonly OnPostprocessSpeedTreeEventClass OnPostprocessSpeedTreeEvent = new OnPostprocessSpeedTreeEventClass();
        private void OnPostprocessSpeedTree(GameObject go)
        {
            OnPostprocessSpeedTreeEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessSpritesEventClass: UnityEvent<Texture2D, Sprite[]>{}
        public static readonly OnPostprocessSpritesEventClass OnPostprocessSpritesEvent = new OnPostprocessSpritesEventClass();
        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            OnPostprocessSpritesEvent.Invoke(texture, sprites);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessTextureEventClass: UnityEvent<Texture2D>{}
        public static readonly OnPostprocessTextureEventClass OnPostprocessTextureEvent = new OnPostprocessTextureEventClass();
        private void OnPostprocessTexture(Texture2D texture)
        {
            OnPostprocessTextureEvent.Invoke(texture);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessTexture2DArrayEventClass: UnityEvent<Texture2DArray>{}
        public static readonly OnPostprocessTexture2DArrayEventClass OnPostprocessTexture2DArrayEvent = new OnPostprocessTexture2DArrayEventClass();
        private void OnPostprocessTexture2DArray(Texture2DArray texture)
        {
            OnPostprocessTexture2DArrayEvent.Invoke(texture);
            OnAnyEvent.Invoke();
        }

        public class OnPostprocessTexture3DEventClass: UnityEvent<Texture3D>{}
        public static readonly OnPostprocessTexture3DEventClass OnPostprocessTexture3DEvent = new OnPostprocessTexture3DEventClass();
        private void OnPostprocessTexture3D(Texture3D texture)
        {
            OnPostprocessTexture3DEvent.Invoke(texture);
            OnAnyEvent.Invoke();
        }
    }
}
