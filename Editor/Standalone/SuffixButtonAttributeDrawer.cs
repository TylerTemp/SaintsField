using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Utils;
using ExtInspector.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Standalone
{
    [CustomPropertyDrawer(typeof(SuffixButtonAttribute))]
    public class SuffixButtonAttributeDrawer: PropertyDrawer
    {
        private Texture _cachedTexture;

        private void InitTexture(string path, float height)
        {
            if (_cachedTexture)
            {
                return;
            }

            Texture2D oriTexture2D = (Texture2D)EditorGUIUtility.Load(path);
            Texture2D compatibleTexture2 = Tex.ConvertToCompatibleFormat(oriTexture2D);
            Tex.ResizeHeightTexture(compatibleTexture2, Mathf.CeilToInt(height));
            _cachedTexture = compatibleTexture2;

            Debug.Assert(_cachedTexture, path);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using EditorGUI.PropertyScope propertyScoop =  new EditorGUI.PropertyScope(position, label, property);

            SuffixButtonAttribute targetAttribute = (SuffixButtonAttribute)attribute;

            if (targetAttribute.Icon != null && Event.current.type == EventType.Repaint)
            {
                InitTexture(targetAttribute.Icon, position.height);
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            GUIContent buttonContent = new GUIContent(targetAttribute.Content, _cachedTexture);
            float buttonWidth = buttonStyle.CalcSize(buttonContent).x;

            (Rect propRect, Rect buttonRect) =
                RectUtils.SplitWidthRect(position, position.width - buttonWidth);

            EditorGUI.PropertyField(propRect, property, propertyScoop.content);
            if (GUI.Button(buttonRect, buttonContent, buttonStyle))
            {
                Object target = property.serializedObject.targetObject;
                MethodInfo matchedMethod = ReflectUil.GetSelfAndBaseTypes(property.serializedObject.targetObject)
                    .SelectMany(systemType => systemType
                        .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                    BindingFlags.Public | BindingFlags.DeclaredOnly))
                    .First(each => each.Name == targetAttribute.Callback);
                ParameterInfo[] methodParams = matchedMethod.GetParameters();
                matchedMethod.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
            }

            // base.OnGUI(position, property, label);
        }

        ~SuffixButtonAttributeDrawer()
        {
            if (_cachedTexture != null)
            {
                Object.DestroyImmediate(_cachedTexture);
            }
        }
    }
}
