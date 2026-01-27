using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer
    {
        public class Wrap: VisualElement
        {
            private object _value;
            private IReadOnlyList<object> _targets;
            private readonly UIToolkitUtils.DropdownButtonField _buttonField;
            private readonly Type _type;
            private readonly DropdownAttribute _attribute;
            private readonly IRichTextTagProvider _richTextTagProvider;
            private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

            public Wrap(string label, DropdownAttribute attribute, Type type, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IRichTextTagProvider richTextTagProvider)
            {
                _type = type;
                _attribute = attribute;
                _richTextTagProvider = richTextTagProvider;

                UIToolkitUtils.DropdownButtonField dropdownButtonField = UIToolkitUtils.MakeDropdownButtonUIToolkit(label);
                Add(dropdownButtonField);
                _buttonField = dropdownButtonField;
                UIToolkitUtils.UIToolkitValueEditAfterProcess(dropdownButtonField, setterOrNull,
                    labelGrayColor, inHorizontalLayout);

                HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                        display = DisplayStyle.None,
                    },
                };
                Add(helpBox);

                dropdownButtonField.ButtonElement.clicked += () =>
                {
                    AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfoShowInInspector(type, attribute, _value, _targets[0], false);

                    (Rect wb, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(dropdownButtonField.worldBound);

                    SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                        metaInfo,
                        dropdownButtonField.worldBound.width,
                        maxHeight,
                        false,
                        (curItem, _) =>
                        {
                            setterOrNull?.Invoke(curItem);
                            return null;
                        }
                    );

                    UnityEditor.PopupWindow.Show(wb, sa);
                    // Add(sa.DebugGetElement());

                    string curError = metaInfo.Error;
                    UIToolkitUtils.SetHelpBox(helpBox, curError);
                };
            }

            public void UpdateValue(object value, IReadOnlyList<object> targets)
            {
                _value = value;
                _targets = targets;
                RefreshButtonLabel();
            }

            private void RefreshButtonLabel()
            {
                AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfoShowInInspector(_type, _attribute, _value, _targets[0], false);
                string display = string.Join("/", metaInfo.SelectStacks.Skip(1).Select(each => each.Display).Append(metaInfo.CurDisplay));
                UIToolkitUtils.SetLabel(_buttonField.ButtonLabelElement, RichTextDrawer.ParseRichXmlWithProvider(display, _richTextTagProvider), _richTextDrawer);
            }
        }

        public static VisualElement UIToolkitValueEdit(VisualElement oldElement, DropdownAttribute treeDropdownAttribute, string label, Type valueType, object value, Action<object> beforeSet, Action<object> setterOrNull, bool labelGrayColor, bool inHorizontalLayout, IReadOnlyList<Attribute> allAttributes, IReadOnlyList<object> targets, IRichTextTagProvider richTextTagProvider)
        {
            if (valueType.BaseType == typeof(Enum) || value is Enum)
            {
                return DrawEnumUIToolkit(oldElement, label, valueType, value, beforeSet, setterOrNull, labelGrayColor,
                    inHorizontalLayout);
            }

            if (oldElement is Wrap oldWrap)
            {
                oldWrap.UpdateValue(value, targets);
                return null;
            }

            Type useType = value?.GetType() ?? valueType;

            Wrap r = new Wrap(label, treeDropdownAttribute, useType, setterOrNull, labelGrayColor, inHorizontalLayout, richTextTagProvider);
            r.UpdateValue(value, targets);
            return r;
        }
    }
}
