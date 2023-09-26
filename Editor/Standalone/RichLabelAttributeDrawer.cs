using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor;
using ExtInspector.Editor.Utils;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Standalone.Editor
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: PropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            RichLabelAttribute targetAttribute = (RichLabelAttribute)attribute;

            (Rect labelRect, Rect propertyRect) =
                RectUtils.SplitWidthRect(EditorGUI.IndentedRect(position), EditorGUIUtility.labelWidth);

            // if(_cachedResult is null)
            // {
            //     string callbackName = targetAttribute.CallbackName;
            //
            //     object target = property.serializedObject.targetObject;
            //     List<Type> types = Util.GetSelfAndBaseTypes(target);
            //     MethodInfo matchedMethod = types
            //         .SelectMany(systemType => systemType
            //             .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
            //                         BindingFlags.Public | BindingFlags.DeclaredOnly))
            //         .First(each => each.Name == callbackName);
            //     ParameterInfo[] methodParams = matchedMethod.GetParameters();
            //
            //     IReadOnlyList<RichText.RichTextPayload> results =
            //         (IReadOnlyList<RichText.RichTextPayload>)matchedMethod.Invoke(target,
            //             methodParams.Select(p => p.DefaultValue).ToArray());
            //     _cachedResult = results;
            // }
            // _richTextDrawer.DrawLabel(labelRect, label, _cachedResult);

            string labelXml = targetAttribute.RichTextXml;
// #if EXT_INSPECTOR_LOG
//             Debug.Log($"RichLabelAttributeDrawer: {labelXml}");
// #endif

            _richTextDrawer.DrawChunks(labelRect, label, RichTextDrawer.ParseRichXml(labelXml, label.text));

            // List<RichTextDrawer.RichTextChunk> parsedChunk = RichTextDrawer.ParseRichXml(labelXml, label.text).ToList();
            //
            // foreach (var richTextChunk in RichTextDrawer.ParseRichXml(labelXml, label.text))
            // {
            //     Debug.Log(richTextChunk.ToString());
            // }

            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
