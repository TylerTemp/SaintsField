using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace SaintsField.Editor.Drawers.LayerDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(LayerAttribute), true)]
    public partial class LayerAttributeDrawer: SaintsPropertyDrawer, IAutoRunnerFixDrawer
    {
        private struct LayerInfo
        {
            public readonly string Name;
            public readonly int Value;

            public LayerInfo(string name, int value)
            {
                Name = name;
                Value = value;
            }
        }

        // Copy from: LayerField.GetAllLayers
        private static IReadOnlyList<LayerInfo> GetAllLayers()
        {
            List<LayerInfo> resultList = new List<LayerInfo>();
            for (int layer = 0; layer < 32; ++layer)
            {
                string layerName = InternalEditorUtility.GetLayerName(layer);
                if (layerName.Length != 0)
                {
                    resultList.Add(new LayerInfo(layerName, layer));
                }
            }
            return resultList;
        }

        private static string GetErrorMessage(SerializedProperty property)
        {
            IReadOnlyList<LayerInfo> allLayers = GetAllLayers();
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int curValue = property.intValue;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (LayerInfo layerInfo in allLayers)
                {
                    // ReSharper disable once InvertIf
                    if(layerInfo.Value == curValue)
                    {
                        return string.Empty;
                    }
                }
                return $"Layer {curValue} is not a valid layer number";
            }

            string curName = property.stringValue;
            if (string.IsNullOrEmpty(curName))
            {
                return string.Empty;
            }
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (LayerInfo layerInfo in allLayers)
            {
                // ReSharper disable once InvertIf
                if(layerInfo.Name == curName)
                {
                    return string.Empty;
                }
            }
            return $"Layer {curName} is not a valid layer name";
        }

        public AutoRunnerFixerResult AutoRunFix(PropertyAttribute propertyAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            SerializedProperty property, MemberInfo memberInfo, object parent)
        {

            string errorMessage;
            try
            {
                errorMessage = GetErrorMessage(property);
            }
            catch (Exception e)
            {
                return new AutoRunnerFixerResult
                {
                    ExecError = e.Message,
                    Error = "",
                };
            }
            if (string.IsNullOrEmpty(errorMessage))
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                ExecError = "",
                Error = errorMessage,
            };
        }
    }
}
