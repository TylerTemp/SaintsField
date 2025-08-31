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
        private Texture2D GetPreview(Object target)
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

            // if (property.propertyType != SerializedPropertyType.ObjectReference ||
            //     property.objectReferenceValue == null)
            // {
            //     return null;
            // }
            // Debug.Log($"check preview {_previewTexture?.width}");
            if(_previewTexture == null || _previewTexture.width == 1)
            {
                // Debug.Log($"load preview {target}");
                if(AssetPreview.IsLoadingAssetPreview(target.GetInstanceID()))
                {
                    return null;
                }

                try
                {
                    _previewTexture = AssetPreview.GetAssetPreview(target);
                }
                catch (AssertionException)  // Unity: Assertion failed on expression: 'i->previewArtifactID == found->second.previewArtifactID'
                {
                    return null;
                }
            }

            if (_previewTexture == null || _previewTexture.width == 1)
            {
                return null;
            }

            return _previewTexture;

            // bool widthOk = width == -1 || _previewTexture.width <= viewWidth;
            // bool heightOk = maxHeight == -1 && _previewTexture.height <= maxHeight;
            // if (widthOk && heightOk)
            // {
            //     return _cachedWidthTexture = _previewTexture;
            // }
            //
            // // Debug.Log($"viewWidth={viewWidth}, width={width}, maxHeight={maxHeight}, _previewTexture.width={_previewTexture.width}, _previewTexture.height={_previewTexture.height}");
            // (int scaleWidth, int scaleHeight) = Tex.GetProperScaleRect(Mathf.FloorToInt(viewWidth), width, maxHeight, _previewTexture.width, _previewTexture.height);
            // // Debug.Log($"scaleWidth={scaleWidth}, scaleHeight={scaleHeight}");
            //
            // if (_cachedWidth == scaleWidth && _cachedHeight == scaleHeight && _cachedWidthTexture != null && _cachedWidthTexture.width != 1 && _cachedWidthTexture.height != 1)
            // {
            //     return _cachedWidthTexture;
            // }
            // _cachedWidth = scaleWidth;
            // _cachedHeight = scaleHeight;
            // // return _cachedWidthTexture = formatted;
            //
            // // _cachedWidthTexture = Tex.TextureTo(_previewTexture, scaleWidth, scaleHeight);
            // _cachedWidthTexture = _previewTexture;
            //
            // if (_cachedWidthTexture.width == 1)
            // {
            //     return _previewTexture;
            // }
            //
            // return _cachedWidthTexture;
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
