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

namespace SaintsField.Editor.Drawers.ButtonDrawers.BelowButtonDrawer
{
    public partial class BelowButtonAttributeDrawer
    {
        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement btn = DrawUIToolkit(property, saintsAttribute, index, info, parent, container);
            btn.style.flexGrow = 1;
            btn.style.flexShrink = 1;
            return btn;
        }


        protected override void CleanResult(VisualElement container, SerializedProperty property, int index)
        {
            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            fancyButton.ClearResult();
        }

        protected override void AppendErrorResult(VisualElement container, SerializedProperty property, int index, string error)
        {
            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            fancyButton.ShowResult(true).Add(MakeErrorBox(error));
        }

        protected override void AppendInvokeResult(VisualElement container, SerializedProperty property, int index, MethodInfo methodInfo, object parent, object result)
        {
            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            VisualElement returnValueContainer = fancyButton.ShowResult(true);

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
                    // fo.value = true;
                }
                fancyButton.ShowResult(true);
                returnValueContainer.Add(r);
            }
        }

        protected override bool HasResult(VisualElement container, SerializedProperty property, int index)
        {
            FancyButton fancyButton = container.Q<FancyButton>(NameButton(property, index));
            return fancyButton.HasResult();
        }
    }
}
#endif
