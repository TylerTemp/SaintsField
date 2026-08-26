using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer.DecoratorRenderer
{
    public partial class DecoratorDrawerRenderer
    {
        private DecoratorDrawer _decoratorDrawerIMGUI;

        public override void OnDestroyIMGUI()
        {
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            return preCheckResult.IsShown ? GetDecoratorDrawerIMGUI().GetHeight() : 0f;
        }

        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            if (!preCheckResult.IsShown)
            {
                return;
            }

            DecoratorDrawer decoratorDrawer = GetDecoratorDrawerIMGUI();
            decoratorDrawer.OnGUI(new Rect(position)
            {
                height = decoratorDrawer.GetHeight(),
            });
        }

        private DecoratorDrawer GetDecoratorDrawerIMGUI()
        {
            if (_decoratorDrawerIMGUI != null)
            {
                return _decoratorDrawerIMGUI;
            }

            _decoratorDrawerIMGUI = (DecoratorDrawer)Activator.CreateInstance(_decoratorDrawerType);
            FieldInfo attributeField = _decoratorDrawerType.GetField("m_Attribute",
                BindingFlags.NonPublic | BindingFlags.Instance);
            attributeField?.SetValue(_decoratorDrawerIMGUI, _propertyAttribute);
            return _decoratorDrawerIMGUI;
        }
    }
}
