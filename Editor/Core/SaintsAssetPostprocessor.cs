using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Core
{
    public class SaintsAssetPostprocessor: AssetPostprocessor
    {
        public static readonly UnityEvent OnAnyEvent = new UnityEvent();

        public static readonly UnityEvent<Material, Renderer> OnAssignMaterialModelEvent = new UnityEvent<Material, Renderer>();
        private Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            OnAssignMaterialModelEvent.Invoke(material, renderer);
            OnAnyEvent.Invoke();
            return material;
        }

        public static readonly UnityEvent<string[], string[], string[], string[]> OnPostprocessAllAssetsEvent = new UnityEvent<string[], string[], string[], string[]>();
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            OnPostprocessAllAssetsEvent.Invoke(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject, AnimationClip> OnPostprocessAnimationEvent = new UnityEvent<GameObject, AnimationClip>();
        private void OnPostprocessAnimation(GameObject go, AnimationClip ac)
        {
            OnPostprocessAnimationEvent.Invoke(go, ac);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<string, string, string> OnPostprocessAssetbundleNameChangedEvent = new UnityEvent<string, string, string>();
        private void OnPostprocessAssetbundleNameChanged(string a, string b, string c)
        {
            OnPostprocessAssetbundleNameChangedEvent.Invoke(a, b, c);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<AudioClip> OnPostprocessAudioEvent = new UnityEvent<AudioClip>();
        private void OnPostprocessAudio(AudioClip ac)
        {
            OnPostprocessAudioEvent.Invoke(ac);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<Cubemap> OnPostprocessCubemapEvent = new UnityEvent<Cubemap>();
        private void OnPostprocessCubemap(Cubemap cm)
        {
            OnPostprocessCubemapEvent.Invoke(cm);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject, EditorCurveBinding[]> OnPostprocessGameObjectWithAnimatedUserPropertiesEvent = new UnityEvent<GameObject, EditorCurveBinding[]>();
        private void OnPostprocessGameObjectWithAnimatedUserProperties(GameObject go, EditorCurveBinding[] bindings)
        {
            OnPostprocessGameObjectWithAnimatedUserPropertiesEvent.Invoke(go, bindings);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject,string[],object[]> OnPostprocessGameObjectWithUserPropertiesEvent = new UnityEvent<GameObject,string[],object[]>();
        private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propertyNames, object[] values)
        {
            OnPostprocessGameObjectWithUserPropertiesEvent.Invoke(go, propertyNames, values);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<Material> OnPostprocessMaterialEvent = new UnityEvent<Material>();
        private void OnPostprocessMaterial(Material material)
        {
            OnPostprocessMaterialEvent.Invoke(material);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject> OnPostprocessMeshHierarchyEvent = new UnityEvent<GameObject>();
        private void OnPostprocessMeshHierarchy(GameObject go)
        {
            OnPostprocessMeshHierarchyEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject> OnPostprocessModelEvent = new UnityEvent<GameObject>();
        private void OnPostprocessModel(GameObject go)
        {
            OnPostprocessModelEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject> OnPostprocessPrefabEvent = new UnityEvent<GameObject>();
        private void OnPostprocessPrefab(GameObject go)
        {
            OnPostprocessPrefabEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<GameObject> OnPostprocessSpeedTreeEvent = new UnityEvent<GameObject>();
        private void OnPostprocessSpeedTree(GameObject go)
        {
            OnPostprocessSpeedTreeEvent.Invoke(go);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<Texture2D, Sprite[]> OnPostprocessSpritesEvent = new UnityEvent<Texture2D, Sprite[]>();
        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            OnPostprocessSpritesEvent.Invoke(texture, sprites);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<Texture2D> OnPostprocessTextureEvent = new UnityEvent<Texture2D>();
        private void OnPostprocessTexture(Texture2D texture)
        {
            OnPostprocessTextureEvent.Invoke(texture);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<Texture2DArray> OnPostprocessTexture2DArrayEvent = new UnityEvent<Texture2DArray>();
        private void OnPostprocessTexture2DArray(Texture2DArray texture)
        {
            OnPostprocessTexture2DArrayEvent.Invoke(texture);
            OnAnyEvent.Invoke();
        }

        public static readonly UnityEvent<Texture3D> OnPostprocessTexture3DEvent = new UnityEvent<Texture3D>();
        private void OnPostprocessTexture3D(Texture3D texture)
        {
            OnPostprocessTexture3DEvent.Invoke(texture);
            OnAnyEvent.Invoke();
        }
    }
}
