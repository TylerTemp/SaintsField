#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        private PropertyField _result;

        private class UserDataPayload
        {
            public string XML;
            public Label Label;
            public string FriendlyName;
            public RichTextDrawer RichTextDrawer;

            public bool TableHasSize;
        }

        private VisualElement _fieldElement;
        private bool _arraySizeCondition;
        private bool _richLabelCondition;

        private static string NameTableContainer(SerializedProperty property)
        {
            return $"saints-table-container-{property.propertyPath}";
        }

        protected override (VisualElement target, bool needUpdate) CreateSerializedUIToolkit()
        {
            VisualElement result = new PropertyField(FieldWithInfo.SerializedProperty)
            {
                style =
                {
                    flexGrow = 1,
                },
                name = FieldWithInfo.SerializedProperty.propertyPath,
            };
            return (result, false);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        // private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition, bool richLabelCondition, FieldInfo info, object parent)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit(root);

            if(_arraySizeCondition)
            {

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
                Debug.Log(
                    $"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; preCheckResult.ArraySize={preCheckResult.ArraySize}, curSize={FieldWithInfo.SerializedProperty.arraySize}");
#endif
                if (preCheckResult.ArraySize != -1 &&
                    ((preCheckResult.ArraySize == 0 && FieldWithInfo.SerializedProperty.arraySize > 0)
                     || (preCheckResult.ArraySize >= 1 && FieldWithInfo.SerializedProperty.arraySize == 0)))
                {
                    FieldWithInfo.SerializedProperty.arraySize = preCheckResult.ArraySize;
                    FieldWithInfo.SerializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            if (_richLabelCondition)
            {
                string xml = preCheckResult.RichLabelXml;
                // Debug.Log(xml);
                UserDataPayload userDataPayload = (UserDataPayload) _fieldElement.userData;
                if (xml != userDataPayload.XML)
                {
                    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                    if (userDataPayload.RichTextDrawer == null)
                    {
                        userDataPayload.RichTextDrawer = new RichTextDrawer();
                    }
                    if(userDataPayload.Label == null)
                    {
                        UIToolkitUtils.WaitUntilThenDo(
                            _fieldElement,
                            () =>
                            {
                                Label label = _fieldElement.Q<Label>(className: "unity-label");
                                if (label == null)
                                {
                                    return (false, null);
                                }
                                return (true, label);
                            },
                            label =>
                            {
                                userDataPayload.Label = label;
                            }
                        );
                    }
                    else
                    {
                        userDataPayload.XML = xml;
                        UIToolkitUtils.SetLabel(userDataPayload.Label, RichTextDrawer.ParseRichXml(xml, userDataPayload.FriendlyName, GetMemberInfo(FieldWithInfo), FieldWithInfo.Target), userDataPayload.RichTextDrawer);
                    }
                }
            }

            return preCheckResult;
        }
    }
}
#endif
