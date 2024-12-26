using System;
using SaintsField.Editor.Drawers;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.SaintsEditorWindowUtils
{
    public partial class WindowInlineEditorRenderer
    {
        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            // if (Event.current.type != EventType.Layout)
            // {
            //     return;
            // }
            Object target = SerializedUtils.GetSerObject(_fieldWithInfo.SerializedProperty,
                _fieldWithInfo.FieldInfo, _fieldWithInfo.Target);

            // Debug.Log(target);

            if (Util.IsNull(target))
            {
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using(SerializedObject so = new SerializedObject(target))
            using(new ExpandableIMGUIScoop())
            {
                // using(EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                // {
                foreach (SerializedProperty iterator in SerializedUtils.GetAllField(so))
                {
                    try
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                }

                    // if(changed.changed)
                    // {
                    so.ApplyModifiedProperties();
                    // }
                // }
            }
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            throw new System.NotSupportedException();
        }

        protected override void RenderPositionTarget(Rect position, PreCheckResult preCheckResult)
        {
            throw new System.NotSupportedException();
        }
    }
}
