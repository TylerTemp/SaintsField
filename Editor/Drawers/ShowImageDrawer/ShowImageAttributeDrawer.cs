using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using UnityEngine.AddressableAssets;
#endif

namespace SaintsField.Editor.Drawers.ShowImageDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ShowImageAttribute), true)]
    [CustomPropertyDrawer(typeof(AboveImageAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowImageAttribute), true)]
    public partial class ShowImageAttributeDrawer : SaintsPropertyDrawer
    {
        private (string error, object imageSource) GetImageSource(SerializedProperty property, string name,
            FieldInfo info, object target)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    (string _, MemberInfo _, object propValue) =
                        Util.GetOf<object>(property.name, null, property, info, target, null);
                    if (propValue is IWrapProp wrapProp)
                    {
                        return GetImageSourceFromTarget(Util.GetWrapValue(wrapProp));
                    }
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
                    if (propValue is AssetReference ar)
                    {
                        return GetImageSourceFromTarget(ar.editorAsset);
                    }
#endif
                    return ($"property {property.propertyPath} is not supported.", null);
                }

                if (property.propertyType != SerializedPropertyType.ObjectReference)
                {
                    return ($"Expect ObjectReference for `{property.propertyPath}`, get {property.propertyType}", null);
                }

                return GetImageSourceFromTarget(GetCurObject(property, info, target));
            }

            bool useFieldHierarchy = name.StartsWith("./");
            bool useCurrentHierarchy = name.StartsWith("/");
            if (useFieldHierarchy || useCurrentHierarchy)
            {
                GameObject startingGo;
                string hierarchyPath;
                if (useFieldHierarchy)
                {
                    (string curError, int _, object curValue) = Util.GetValue(property, info, target);
                    if (curError != "")
                    {
                        return ($"Fail to get value of `{property.propertyPath}`: {curError}", null);
                    }

                    if (RuntimeUtil.IsNull(curValue))
                    {
                        return ("", null);
                    }

                    switch (curValue)
                    {
                        case Component comp:
                            startingGo = comp.gameObject;
                            break;
                        case GameObject go:
                            startingGo = go;
                            break;
                        default:
                            return ($"Field value {curValue} is not GameObject or Component", null);
                    }

                    hierarchyPath = name.Substring(2);
                }
                else
                {
                    switch (target)
                    {
                        case Component comp:
                            startingGo = comp.gameObject;
                            break;
                        case GameObject go:
                            startingGo = go;
                            break;
                        default:
                            return ($"Target object {target} is not GameObject or Component", null);
                    }

                    hierarchyPath = name.Substring(1);
                }

                Transform findChild = startingGo.transform.Find(hierarchyPath);
                if (!findChild)
                {
                    return ($"Fail to find child `{hierarchyPath}` under `{startingGo.name}`", null);
                }

                return GetImageSourceFromTarget(findChild.gameObject);
            }

            (string reflectError, MemberInfo _, object fieldValue) =
                Util.GetOf<object>(name, null, property, info, target, null);
            if (reflectError == "")
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

                return GetImageSourceFromTarget(fieldValue);
            }

            return ($"not found `{name}` on `{target}`", null);
        }

        private static Object GetCurObject(SerializedProperty property, FieldInfo info, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
            {
                return property.objectReferenceValue;
            }

            (string error, int _, object propertyValue) = Util.GetValue(property, info, parent);
            return error == "" && propertyValue is IWrapProp wrapProp
                ? Util.GetWrapValue(wrapProp) as Object
                : null;
        }

        private static (string error, object imageSource) GetImageSourceFromTarget(object result)
        {
            if (RuntimeUtil.IsNull(result))
            {
                return ("", null);
            }

            switch (result)
            {
                case Sprite sprite:
                    return ("", sprite);
                case Texture2D texture2D:
                    return ("", texture2D);
                case SpriteRenderer spriteRenderer:
                    return ("", spriteRenderer.sprite);
                case Image image:
                    return ("", image.sprite);
                case RawImage image:
                    return ("", image.texture as Texture2D);
                case Button button:
                    return button.targetGraphic switch
                    {
                        Image targetImage => ("", targetImage.sprite),
                        RawImage targetRawImage => ("", targetRawImage.texture as Texture2D),
                        _ => button.targetGraphic
                            ? ("", button.targetGraphic.mainTexture as Texture2D)
                            : ("", null),
                    };
                case GameObject _:
                case Component _:
                {
                    Object obj = (Object)result;
                    Object actualObj = Util.GetTypeFromObj(obj, typeof(SpriteRenderer))
                                       ?? Util.GetTypeFromObj(obj, typeof(Image))
                                       ?? Util.GetTypeFromObj(obj, typeof(RawImage))
                                       ?? Util.GetTypeFromObj(obj, typeof(Button));
                    return GetImageSourceFromTarget(actualObj);
                }
                default:
                    return ($"Unable to find image on {result.GetType()}", null);
            }
        }
    }
}
