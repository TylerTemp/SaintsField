using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Utils
{
    public static class SaintsFieldConfigSettings
    {
        // [SettingsProvider]
        // public static SettingsProvider CreateProvider()
        // {
        //     return new SettingsProvider("Project/SaintsField", SettingsScope.Project)
        //     {
        //         label = "SaintsField",
        //         activateHandler = (_, root) =>
        //         {
        //
        //         },
        //         keywords = new System.Collections.Generic.HashSet<string>(new[] { "SaintsField" }),
        //     };
        // }

        [SettingsProvider]
        public static SettingsProvider CreateProviderSetup()
        {
            return new SettingsProvider("Project/SaintsField", SettingsScope.Project)
            {
                label = "SaintsField",
                activateHandler = (_, root) =>
                {
                    ScrollView scroller = new ScrollView();
                    root.Add(scroller);

                    SaintsFieldSetupWindow setup = ScriptableObject.CreateInstance<SaintsFieldSetupWindow>();
                    UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(setup);
                    scroller.Add(new InspectorElement(editor));
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "SaintsField" }),
            };
        }

        [SettingsProvider]
        public static SettingsProvider CreateProviderConfig()
        {
            return new SettingsProvider("Project/SaintsField/[0]Config", SettingsScope.Project)
            {
                label = "Config",
                activateHandler = (_, root) =>
                {
                    ScrollView scroller = new ScrollView();
                    root.Add(scroller);

                    Button createConfigButton = new Button
                    {
                        text = "Create Config",
                        style =
                        {
                            display = SaintsFieldConfigUtil.IsConfigLoaded? DisplayStyle.None: DisplayStyle.Flex,
                        },
                    };
                    scroller.Add(createConfigButton);

                    VisualElement configContainer = new VisualElement();
                    scroller.Add(configContainer);

                    createConfigButton.clicked += () =>
                    {
                        AddConfigInspector(configContainer).OnDeleteConfig.AddListener(OnDeleteConfig);
                        createConfigButton.style.display = DisplayStyle.None;
                    };

                    if (SaintsFieldConfigUtil.IsConfigLoaded)
                    {
                        AddConfigInspector(configContainer).OnDeleteConfig.AddListener(OnDeleteConfig);
                    }

                    return;

                    void OnDeleteConfig()
                    {
                        configContainer.Clear();
                        createConfigButton.style.display = DisplayStyle.Flex;
                    }
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "SaintsField" }),
            };
        }

        private static SaintsFieldConfig AddConfigInspector(VisualElement root)
        {
            SaintsFieldConfig config = SaintsMenu.EnsureCreateSaintsFieldConfig();
            // SerializedObject serializedObject = new SerializedObject(config);

            SaintsEditor editor = (SaintsEditor)UnityEditor.Editor.CreateEditor(config, typeof(SaintsEditor));
            editor.EditorShowMonoScript = false;
            InspectorElement ins = new InspectorElement(editor);

            root.Add(ins);

            // root.Bind(serializedObject);
            return config;
        }

        [SettingsProvider]
        public static SettingsProvider CreateProviderEColor()
        {
            return new SettingsProvider("Project/SaintsField/[1]EColor Preview", SettingsScope.Project)
            {
                label = "EColor Preview",
                activateHandler = (_, root) =>
                {
                    ScrollView scroller = new ScrollView();
                    root.Add(scroller);

                    EColorPreviewWindow window = ScriptableObject.CreateInstance<EColorPreviewWindow>();
                    UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(window);
                    scroller.Add(new InspectorElement(editor));
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "SaintsField" }),
            };
        }
    }
}
