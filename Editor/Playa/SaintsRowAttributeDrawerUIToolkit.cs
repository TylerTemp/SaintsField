#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa
{
    public partial class SaintsRowAttributeDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();
            FillElement(root, property);

            // if (property.propertyType == SerializedPropertyType.ManagedReference)
            // {
            //     var curId = property.managedReferenceId;
            //     root.schedule.Execute(() =>
            //     {
            //         if (curId != property.managedReferenceId)
            //         {
            //             curId = property.managedReferenceId;
            //             root.Clear();
            //             FillElement(root, property.Copy());
            //         }
            //         // Debug.Log(property.managedReferenceId);
            //     })
            //     .Every(100);
            // }
            return root;
        }

        private void FillElement(VisualElement root, SerializedProperty property)
        {
            object value;
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                value = property.managedReferenceValue;
                if (value == null)
                {
                    return;
                }
            }
            else
            {
                object parentValue = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                (string error, int _, object getValue) = Util.GetValue(property, fieldInfo, parentValue);
                if (error != "")
                {
                    root.Add(new HelpBox(error, HelpBoxMessageType.Error));
                    return;
                }

                value = getValue;
            }

            Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                .ToDictionary(each => each.name, each => each.property);

            SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute)attribute;

            IReadOnlyList<ISaintsRenderer> renderer =
                SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, this, value);

            // VisualElement bodyElement = SaintsEditor.CreateVisualElement(renderer);
            VisualElement bodyElement = new VisualElement();
            foreach (ISaintsRenderer saintsRenderer in renderer)
            {
                VisualElement rendererElement = saintsRenderer.CreateVisualElement();
                if (rendererElement != null)
                {
                    bodyElement.Add(rendererElement);
                }
            }

#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
            bodyElement.RegisterCallback<AttachToPanelEvent>(_ => SaintsEditor.AddInstance(this));
            bodyElement.RegisterCallback<DetachFromPanelEvent>(_ => SaintsEditor.RemoveInstance(this));
#endif

            if (saintsRowAttribute.Inline)
            {
                root.Add(bodyElement);
                return;
            }

            bodyElement.style.paddingLeft = SaintsPropertyDrawer.IndentWidth;

            Foldout toggle = new Foldout
            {
                text = property.displayName,
                value = true,
            };
            toggle.RegisterValueChangedCallback(evt =>
                bodyElement.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            root.Add(toggle);
            root.Add(bodyElement);

        }
    }
}
#endif
