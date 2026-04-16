#if UNITY_EDITOR
using System;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue379
{
    public class CustomEditorWindow: SaintsEditorWindow
    {
        // [MenuItem("SaintsField/DebugIssue379")]
        private static void OpenWindow()
        {
            EditorWindow window = GetWindow<CustomEditorWindow>(false, "Debug Issue 379");
            window.Show();
        }

        [Serializable]
        public class BVC
        {
            public string myBvc;

            [Serializable]
            public class SimData
            {
                public string mySimData;
            }
        }

        [SaintsRow(true)] public BVC bvc;

        [HideIf(true)]
        public BVC.SimData simData;


        protected override void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

            rootVisualElement.Add(splitView);
            var leftPane = new VisualElement();
            splitView.Add(leftPane);
            var rightPane = new VisualElement();
            splitView.Add(rightPane);

            leftPane.Add(EditorCreatInspectingTarget());
        }

        public override void CreateGUI()
        {
            base.CreateGUI();
            var container = new IMGUIContainer(() => { Gui(); });
            rootVisualElement[0][1].Add(container);
        }

        private void Gui()
        {
            using (SerializedObject serObj = new SerializedObject(GetTarget()))
            {
                var property = serObj.FindProperty(nameof(simData));

                // EditorGUILayout.PropertyField(property, true);

                using (var changed = new EditorGUI.ChangeCheckScope())
                {
                    // This is where you draw the charts. PropertyField won't work here because `HideIf`
                    // Change it to your own drawing logic
                    GuiCharts();
                    EditorGUILayout.PropertyField(property, true);
                    if (changed.changed)
                    {
                        serObj.ApplyModifiedProperties();
                    }
                }
            }


        }

        public void GuiCharts()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Comparison Chart", EditorStyles.boldLabel);
            GUILayout.Space(80);

            Rect chartRect = GUILayoutUtility.GetRect(400, 250); // Chartdimensions

            EditorGUI.DrawRect(chartRect, Color.blue);
            //     // --- Draw the chart
            //     _chartDrawer.DrawChart(
            //         chartRect, // Area where the chart is rendered
            //         ChartData_Series, // Y-axis data
            //         "Index (X)", // X-axis label
            //         i => $"#{simData.InputRangeMin + i}", // X-axis value formatter
            //         v => v.ToString("F1"), // Y-axis value formatter (1decimal)
            //         (s, idx) => new[] { $"Value: {s[idx]:F1}" }, // Tooltipcontent
            //         ChartBuilder.Scripts.Editor.ChartDrawerXY.ChartType.Line,
            //
            // // Chart type
            //         ChartData_Names, // Names of each data series
            //         ChartData_Colors, // Custom colors for the series
            //         8, // Number of X-axis grid lines
            //         7, // Number of Y-axis grid lines,
            //         i => $"# {simData.InputRangeMin + i}"
            //     );
            GUILayout.EndVertical();
        }
    }
}
#endif
