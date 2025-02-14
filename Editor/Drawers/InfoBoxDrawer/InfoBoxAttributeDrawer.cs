using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.Drawers.InfoBoxDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(InfoBoxAttribute), true)]
    [CustomPropertyDrawer(typeof(BelowInfoBoxAttribute), true)]
    public partial class InfoBoxAttributeDrawer: SaintsPropertyDrawer
    {
        // private bool _overrideMessageType;
        // private EMessageType _messageType;

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public EMessageType MessageType;
            public string Content;
            public bool WillDrawBox;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, InfoBoxAttribute infoboxAttribute,
            FieldInfo info, object target)
        {
            (string drawError, bool willDraw) = WillDraw(property, infoboxAttribute, info, target);
            if (drawError != "")
            {
                return new MetaInfo
                {
                    Error = drawError,
                };
            }

            if (!infoboxAttribute.IsCallback)
            {
                return new MetaInfo
                {
                    Content = infoboxAttribute.Content,
                    MessageType = infoboxAttribute.MessageType,
                    WillDrawBox = infoboxAttribute.Content != null && willDraw,
                    Error = "",
                };
            }

            (string error, object result) = Util.GetOf<object>(
                infoboxAttribute.Content,
                null,
                property,
                info,
                target);

            if (error != "")
            {
                return new MetaInfo
                {
                    Error = error,
                };
            }

            if (result is ValueTuple<EMessageType, string> resultTuple)
            {
                return new MetaInfo
                {
                    Error = "",
                    WillDrawBox = resultTuple.Item2 != null && willDraw,
                    MessageType = resultTuple.Item1,
                    Content = resultTuple.Item2,
                };
            }

            // Debug.Log($"result={result}, null={result == null}, willDraw={willDraw}/{property.propertyPath}/{info.Name}");

            return new MetaInfo
            {
                Error = "",
                WillDrawBox = result != null && willDraw,
                MessageType = infoboxAttribute.MessageType,
                Content = result == null ? "" : result.ToString(),
            };
        }

        private static (string error, bool willDraw) WillDraw(SerializedProperty property, InfoBoxAttribute infoboxAttribute, FieldInfo info, object target)
        {
            if (infoboxAttribute.ShowCallback == null)
            {
                return ("", true);
            }

            (string error, object result) = Util.GetOf<object>(
                infoboxAttribute.ShowCallback,
                null,
                property,
                info,
                target);

            return error != ""
                ? (error, false)
                : ("", ReflectUtils.Truly(result));
        }
    }
}
