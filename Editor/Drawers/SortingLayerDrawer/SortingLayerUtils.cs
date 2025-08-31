using System;
using System.Reflection;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public static class SortingLayerUtils
    {
        public static void OpenSortingLayerInspector()
        {
            // TagManagerInspector.ShowWithInitialExpansion(TagManagerInspector.InitialExpansionState.Layers)
            Type tagManagerInspectorType = Type.GetType("UnityEditor.TagManagerInspector, UnityEditor");
            // Get the method Info for the ShowWithInitialExpansion method
            if (tagManagerInspectorType == null)
            {
                return;
            }

            MethodInfo showWithInitialExpansionMethod = tagManagerInspectorType.GetMethod("ShowWithInitialExpansion", BindingFlags.Static | BindingFlags.NonPublic);
            if (showWithInitialExpansionMethod == null)
            {
                return;
            }

            Type initialExpansionStateType = tagManagerInspectorType.GetNestedType("InitialExpansionState", BindingFlags.NonPublic);
            object layersEnumValue = Enum.Parse(initialExpansionStateType, "SortingLayers");
            // Invoke the ShowWithInitialExpansion method with the Layers enum value
            showWithInitialExpansionMethod.Invoke(null, new object[] { layersEnumValue });
        }
    }
}
