using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using UnityEngine.AddressableAssets;
#endif

namespace SaintsField.Editor.Drawers.ShowImageDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ShowImageAttribute), true)]
    [CustomPropertyDrawer(typeof(AboveImageAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowImageAttribute), true)]
    public partial class ShowImageAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, Texture2D image) GetImage(SerializedProperty property, string name, FieldInfo info, object target)
        {
            if (string.IsNullOrEmpty(name))
            {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    (string _, object propValue) = Util.GetOf<object>(property.name, null, property, info, target);
                    if (propValue is IWrapProp wrapProp)
                    {
                        object actualValue = Util.GetWrapValue(wrapProp);
                        // Debug.Log(actualValue);
                        return GetImageFromTarget(actualValue);
                    }
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
                    if (propValue is AssetReference ar)
                    {
                        return GetImageFromTarget(ar.editorAsset);
                    }
#endif
                    return ($"property {property.propertyPath} is not supported.", null);
                }

                if(property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    return ($"Expect ObjectReference for `{property.propertyPath}`, get {property.propertyType}", null);
                }

                return GetImageFromTarget(GetCurObject(property, info, target));
            }

            // search parent first
            (string reflectError, object fieldValue) = Util.GetOf<object>(name, null, property, info, target);
            if(reflectError == "")
            {
                if (fieldValue is IWrapProp wrapProp)
                {
                    fieldValue = Util.GetWrapValue(wrapProp);
                }
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
                else if (fieldValue is AssetReference ar)
                {
                    fieldValue = ar.editorAsset;
                }
#endif

                Texture2D reflect2D;
                (reflectError, reflect2D) = GetImageFromTarget(fieldValue);
                if (reflectError == "")
                {
                    return (reflectError, reflect2D);
                }
            }

            // SerializedProperty prop = property.serializedObject.FindProperty(name) ?? SerializedUtils.FindPropertyByAutoPropertyName(property.serializedObject, name);
            // if (prop != null)
            // {
            //     // ReSharper disable once ConvertIfStatementToReturnStatement
            //     if (prop.propertyType == SerializedPropertyType.Generic)
            //     {
            //         GetCurObject(prop, info, parent);
            //     }
            //     else if(prop.propertyType != SerializedPropertyType.ObjectReference)
            //     {
            //         return ($"Expect ObjectReference for `{name}`, get {prop.propertyType}", null);
            //     }
            //
            //     return GetImageFromTarget(prop.objectReferenceValue);
            // }

            return ($"not found `{name}` on `{target}`", null);
        }

        private static Object GetCurObject(SerializedProperty property, FieldInfo info, object parent)
        {
            // Object curObject = null;
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return property.objectReferenceValue;
            }

            (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);

            if (error == "" && propertyValue is IWrapProp wrapProp)
            {
                return Util.GetWrapValue(wrapProp) as Object;
            }

            return null;
        }

        private static (string error, Texture2D image) GetImageFromTarget(object result)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (result)
            {
                case Sprite sprite:
                    return ("", sprite.texture);
                case Texture2D texture2D:
                    return ("", texture2D);
                // case Texture texture:
                //     targetChanged = !ReferenceEquals(_originTexture, texture);
                //     _originTexture = texture as Texture2D;
                //     break;
                case SpriteRenderer spriteRenderer:
                    return ("", spriteRenderer.sprite == null? null: spriteRenderer.sprite.texture);
                case Image image:
                    return ("", image.sprite == null? null: image.sprite.texture);
                case RawImage image:
                    return ("", image.texture as Texture2D);
                case Button button:
                    // targetChanged = !ReferenceEquals(_originTexture, button.targetGraphic.mainTexture);
                    return button.targetGraphic?
                        ("", button.targetGraphic.mainTexture as Texture2D):
                        ("", null);
                case GameObject _:
                case Component _:
                {
                    Object obj = (Object)result;
                    Object actualObj = Util.GetTypeFromObj(obj, typeof(SpriteRenderer))
                                                   ?? Util.GetTypeFromObj(obj, typeof(Image))
                                                   ?? Util.GetTypeFromObj(obj, typeof(RawImage))
                                                   ?? Util.GetTypeFromObj(obj, typeof(Button))
                                                   ;
                    // Debug.Log($"obj={obj} actual={actualObj}, renderer={((Component)foundObj).GetComponent<Renderer>()}");
                    // ReSharper disable once TailRecursiveCall
                    return GetImageFromTarget(
                        actualObj
                    );
                }
                default:
                    return (
                        $"Unable to find image on {(result == null ? "null" : result.GetType().ToString())}",
                        null);
            }
        }

#if UNITY_2021_3_OR_NEWER

        // ReSharper disable once SuggestBaseTypeForParameter

#endif
    }
}
