using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Playa;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.PlayaSeparatorSemiRenderer
{
    public partial class PlayaSeparatorRenderer : AbsRenderer
    {
        protected override bool AllowGuiColor => true;

        private readonly SeparatorAttribute _playaSeparatorAttribute;
        private readonly string _colorHex;

        public PlayaSeparatorRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo,
            SeparatorAttribute playaSeparatorAttribute) : base(serializedObject, fieldWithInfo)
        {
            _playaSeparatorAttribute = playaSeparatorAttribute;
            _colorHex = ColorUtility.ToHtmlStringRGB(_playaSeparatorAttribute.Color);
        }

        public override void OnSearchField(string searchString)
        {
        }

        private (string error, string richXml) GetSeparatorRichXml()
        {
            if (_playaSeparatorAttribute.Title == null)
            {
                return ("", null);
            }

            string richXml = _playaSeparatorAttribute.Title;
            if (_playaSeparatorAttribute.IsCallback)
            {
                (string error, object rawResult) = GetCallback(FieldWithInfo, _playaSeparatorAttribute.Title);
                if (error != "")
                {
                    return (error, null);
                }

                if (rawResult is string rawString)
                {
                    return ("", $"<color=#{_colorHex}>{rawString}</color>");
                }

                if (RuntimeUtil.IsNull(rawResult))
                {
                    return ("", null);
                }

                return ($"{rawResult} is not a string", null);
            }

            if (!string.IsNullOrEmpty(_playaSeparatorAttribute.Title))
            {
                richXml = $"<color=#{_colorHex}>{_playaSeparatorAttribute.Title}</color>";
            }

            return ("", richXml);
        }
    }
}
