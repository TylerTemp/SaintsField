using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.InputAxisDrawer
{
    public static class InputAxisUtils
    {
        public static IReadOnlyList<string> GetAxisNames()
        {
            List<string> axisNames = new List<string>();

            // ReSharper disable once ConvertToUsingDeclaration
            using(SerializedObject inputAssetSettings = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/InputManager.asset")))
            {
                SerializedProperty axesProperty = inputAssetSettings.FindProperty("m_Axes");

                for (int index = 0; index < axesProperty.arraySize; index++)
                {
                    axisNames.Add(axesProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_Name")
                        .stringValue);
                }
            }

            return axisNames;
        }

        public static void OpenInputManager()
        {
            SettingsService.OpenProjectSettings("Project/Input Manager");
        }
    }
}
