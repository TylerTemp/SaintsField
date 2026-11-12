using System;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.Events;
#endif

namespace SaintsField.Editor.Playa.Renderer.DecoratorRenderer
{
    public partial class DecoratorDrawerRenderer: AbsRenderer
    {
        private readonly PropertyAttribute _propertyAttribute;
        private readonly Type _decoratorDrawerType;

        public DecoratorDrawerRenderer(PropertyAttribute propertyAttribute, Type decoratorDrawerType,
            SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _propertyAttribute = propertyAttribute;
            _decoratorDrawerType = decoratorDrawerType;
        }

        protected override bool AllowGuiColor => true;

#if UNITY_2021_3_OR_NEWER
        private System.Func<string, bool> _searchFunc;
#endif

        public override bool OnSearchField(string searchString)
        {
#if UNITY_2021_3_OR_NEWER
            return _searchFunc?.Invoke(searchString) ?? false;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            // ReSharper disable once HeuristicUnreachableCode
            return false;
#pragma warning restore CS0162 // Unreachable code detected
        }

        public override string ToString()
        {
            return $"<Decorator {GetFriendlyName(FieldWithInfo)}/{_decoratorDrawerType.Name}>";
        }
    }
}
