#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ButtonDrawers.PostFieldButtonDrawer
{
    public partial class PostFieldButtonAttributeDrawer
    {
        private static string NameResultPanel(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__Buttonresult";
        private static string NameInvokeResult(SerializedProperty property, int index) => $"{property.propertyPath}__{index}__ButtonInvoke";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement btn = DrawUIToolkit(property, saintsAttribute, index);
            // btn.style.flexGrow = 1;
            // btn.style.flexShrink = 1;
            return btn;
            // return element;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement r = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
                name = NameResultPanel(property, index),
            };
            r.Add(new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
                name = NameInvokeResult(property, index),
            });
            return r;
        }


        protected override void CleanResult(VisualElement container, SerializedProperty property, int index)
        {
            container.Q<VisualElement>(NameResultPanel(property, index)).Clear();
        }

        protected override void AppendErrorResult(VisualElement container, SerializedProperty property, int index, string error)
        {
            container.Q<VisualElement>(NameResultPanel(property, index)).Add(MakeErrorBox(error));
            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            fancyButton.ShowCloseButton(true);
        }



        protected override void AppendInvokeResult(VisualElement container, SerializedProperty property, int index, MethodInfo methodInfo,
            object parent, object result)
        {

            VisualElement elem = container.Q<VisualElement>(NameInvokeResult(property, index));

            VisualElement r = UIToolkitEdit.UIToolkitValueEdit(
                null,
                "<color=green>[return]</color>",
                methodInfo.ReturnType,
                result,
                null,
                _ => { },
                false,
                InHorizontalLayout,
                ReflectCache.GetCustomAttributes(methodInfo),
                new[]{parent},
                this,
                SerializedUtils.GetUniqueId(property)
            ).result;
            if (r != null)
            {
                if (r is Foldout { value: false } fo)
                {
                    fo.RegisterCallback<AttachToPanelEvent>(_ => fo.value = true);
                }
                elem.Add(r);
            }
        }

        protected override bool HasResult(VisualElement container, SerializedProperty property, int index)
        {
            VisualElement elem = container.Q<VisualElement>(NameInvokeResult(property, index));
            return elem.childCount > 0;
        }
    }
}

#endif
