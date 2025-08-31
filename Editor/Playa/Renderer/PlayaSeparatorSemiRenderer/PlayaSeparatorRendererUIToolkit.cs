#if UNITY_2021_3_OR_NEWER // && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SeparatorDrawer;
using SaintsField.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer
    {
        private VisualElement _titleElement;
        private string _richXml;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            (VisualElement target, VisualElement title) result =
                SeparatorAttributeDrawer.CreateSeparatorUIToolkit(_playaSeparatorAttribute, "playa-separator-text");
            // _titleElement = ve.Q(name: "playa-separator-text");
            _titleElement = result.title;
            return (result.target, _playaSeparatorAttribute.Title != null);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult result = UpdatePreCheckUIToolkit();

            if (_playaSeparatorAttribute.Title == null)
            {
                return result;
            }

            string newRichXml = _playaSeparatorAttribute.Title;
            if (_playaSeparatorAttribute.IsCallback)
            {
                (string error, object rawResult) = GetCallback(FieldWithInfo, _playaSeparatorAttribute.Title);
                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    return result;
                }

                if (rawResult is string rawString)
                {
                    newRichXml = $"<color=#{ColorUtility.ToHtmlStringRGBA(_playaSeparatorAttribute.Color)}>{rawString}</color>";
                }
                else if (RuntimeUtil.IsNull(rawResult))
                {
                    newRichXml = null;
                }
#if SAINTSFIELD_DEBUG
                else
                {
                    Debug.LogError($"{rawResult} is not a string");
                    return result;
                }
#endif
            }

            if (newRichXml != _richXml || (newRichXml != null && newRichXml.Contains("<field")))
            {
                // Debug.Log($"newRichXml={newRichXml}, _richXml={_richXml}");
                _richXml = newRichXml;
                // _titleElement.Clear();
                if (!string.IsNullOrEmpty(newRichXml))
                {
                    _titleElement.Clear();
                    IEnumerable<RichTextDrawer.RichTextChunk> chunks = RichTextDrawer.ParseRichXml(
                        newRichXml,
                        GetFriendlyName(FieldWithInfo),
                        FieldWithInfo.SerializedProperty,
                        FieldWithInfo.FieldInfo ?? (MemberInfo)FieldWithInfo.PropertyInfo ?? FieldWithInfo.MethodInfo,
                        FieldWithInfo.Targets[0]);
                    foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(chunks))
                    {
                        // Debug.Log($"title add {rich}");
                        _titleElement.Add(rich);
                    }
                    _titleElement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _titleElement.style.display = DisplayStyle.None;
                }
            }

            return result;
        }
    }
}
#endif
