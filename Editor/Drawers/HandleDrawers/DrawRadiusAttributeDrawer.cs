// using System.Diagnostics;
// using SaintsField.Utils;
// using UnityEngine;
//
// namespace SaintsField.Editor.Drawers.HandleDrawers
// {
//     [Conditional("UNITY_EDITOR")]
//     [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
//
//     public class DrawRadiusAttributeDrawer : PropertyAttribute, ISaintsAttribute
//     {
//         public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
//         public string GroupBy => "";
//
//         public readonly EColor EColor;
//
//         public readonly float Radius = -1f;
//
//         public readonly string Content;
//         public bool IsCallback;
//
//         public DrawRadiusAttributeDrawer(EColor eColor, string content, bool isCallback = false)
//         {
//             EColor = eColor;
//             (string parsedContent, bool parsedIsCallback) = RuntimeUtil.ParseCallback(content, isCallback);
//             Content = parsedContent;
//             IsCallback = parsedIsCallback;
//         }
//
//         public DrawRadiusAttributeDrawer(EColor eColor, float radius)
//         {
//             EColor = eColor;
//             Radius = radius;
//         }
//
//         public DrawRadiusAttributeDrawer(string content, bool isCallback = false): this(EColor.White, content, isCallback)
//         {
//         }
//
//         public DrawRadiusAttributeDrawer(float radius): this(EColor.White, radius)
//         {
//         }
//     }
// }
