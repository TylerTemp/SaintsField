#if (WWISE_2025_OR_LATER || WWISE_2024_OR_LATER || WWISE_2023_OR_LATER || WWISE_2022_OR_LATER || SAINTSFIELD_WWISE) && !SAINTSFIELD_WWISE_DISABLE

#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils.SaintsObjectPickerWindow;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Wwise.GetWwiseDrawer
{
    public partial class GetWwiseAttributeDrawer
    {
        // private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__WwiseAutoGetter";
        //
        // protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, FieldInfo info, object parent)
        // {
        //     return new HelpBox("", HelpBoxMessageType.Error)
        //     {
        //         style =
        //         {
        //             flexGrow = 1,
        //             display = DisplayStyle.None,
        //         },
        //         name = NameHelpBox(property),
        //     };
        // }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            (bool isReadable, string error, SerializedProperty _) =
                GetWwiseObjectReferenceProperty(property, info);
            if (!isReadable)
            {
                return;
            }

            if (error != "")
            {
                HelpBox helpBox = GetHelpBox(container, property, index);
                helpBox.text = error;
                helpBox.style.display = DisplayStyle.Flex;
                return;
            }

            base.OnAwakeUIToolkit(property, saintsAttribute, index, allAttributes, container, onValueChangedCallback, info, parent);
        }

        protected override SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo MakeObjectBaseInfo(UnityEngine.Object objResult,
            string assetPath)
        {
            if (objResult is WwiseObjectReference wwiseObjectReference)
            {
                string path = "";
                string type = "";
                // ReSharper disable once InvertIf
                if (GuidToPath.ContainsKey(wwiseObjectReference.Guid))
                {
                    WwiseBasicInfo value = GuidToPath[wwiseObjectReference.Guid];
                    path = string.Join("/", value.BasicPathSegments);
                    type = value.WwiseObjectType.ToString();
                }
                return new SaintsObjectPickerWindowUIToolkit.ObjectBaseInfo(
                    wwiseObjectReference,
                    wwiseObjectReference.ObjectName,
                    type,
                    path
                );
            }

            if (!objResult)
            {
                return SaintsObjectPickerWindowUIToolkit.NoneObjectInfo;
            }

            throw new ArgumentException($"Unsupported args {objResult}", nameof(objResult));
        }
    }
}

#endif

#endif
