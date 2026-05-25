using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using UnityEngine.AddressableAssets;
#endif

namespace SaintsField.Editor.Drawers.AssetPreviewDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute), true)]
    public partial class AssetPreviewAttributeDrawer : SaintsPropertyDrawer
    {
        private const int InteractiveDefaultSize = 300;

        private static Texture2D GetPreview(Object target)
        {
            // Debug.Log(target);
            if (!target)
            {
                return null;
            }

            if (target is Component c)
            {
                target = c.gameObject;
            }

            Texture2D previewTexture;
            try
            {
                previewTexture = AssetPreview.GetAssetPreview(target);
            }
            catch (AssertionException)  // Unity: Assertion failed on expression: 'i->previewArtifactID == found->second.previewArtifactID'
            {
                return null;
            }

            if(AssetPreview.IsLoadingAssetPreview(target.
#if UNITY_6000_4_OR_NEWER
                   GetEntityId
#else
                   GetInstanceID
#endif
                       ()))

            {
                return null;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(previewTexture == null)
            {
                // Debug.Log($"load preview {target}");
                return null;
            }

            return previewTexture;
        }

        private static string MismatchError(SerializedProperty property, FieldInfo info, object parent)
        {
            // if (property.propertyType != SerializedPropertyType.ObjectReference)
            // {
            //     return $"Expect string or int, get {property.propertyType}";
            // }
            return GetCurObject(property, info, parent) == null
                ? "field is null"
                : null;
        }

        private static Object GetCurObject(SerializedProperty property, FieldInfo info, object parent)
        {
            // Object curObject = null;
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return property.objectReferenceValue;
            }

            (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);
            if (error != "")
            {
                return null;
            }



            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (propertyValue is IWrapProp wrapProp)
            {
                return Util.GetWrapValue(wrapProp) as Object;
            }
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
            if (propertyValue is AssetReference ar)
            {
                return ar.editorAsset;
            }
#endif

            return null;
        }
    }
}
