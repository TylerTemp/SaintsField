#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SaintsField.Editor;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue379
{
    public class BVCSimulationPanel : SaintsEditorWindow
    {
        // private readonly ChartBuilder.Scripts.Editor.ChartDrawerXY
        //     _chartDrawer = new();


        [SaintsRow(true)] public BVC bvc;


        [Separator("Sim Data", EAlign.Center, space: 20)] [LayoutStart("SimData", ELayout.FoldoutBox)] [SaintsRow(true)]
        public BVC.SimData simData_Internal;

        public static BVCSimulationPanel OpenNewWindow(BVC data)
        {
            var window = CreateWindow<BVCSimulationPanel>("bvc sim");
            // Debug.Log(data.resourceType);
            window.bvc = data;
            window.simData_Internal = data.simData;
            window.Show();

            return window;
        }


        public DateTime nextUpdate = new DateTime();

        public override void OnEditorUpdate()
        {
            if (DateTime.Now > nextUpdate)
            {
                ReCalculateChartsData();
                nextUpdate = DateTime.Now.AddSeconds(1);
            }
        }


        public override void OnEditorEnable()
        {
            InitChartInitialData();
        }


        public void InitChartInitialData()
        {
            ChartData_Names = new()
            {
                "Data"
            };
            ChartData_Colors = new()
            {
                Color.cyan
            };

            ChartData_Series_0 = new float[0];
            ChartData_Series = new();
            ChartData_Series.Add(ChartData_Series_0);
        }

        private List<float[]> ChartData_Series;
        private float[] ChartData_Series_0;
        private List<string> ChartData_Names;
        private List<Color> ChartData_Colors;

        public void ReCalculateChartsData()
        {
            if (ChartData_Series_0.Length != simData_Internal.InputRangeMax - simData_Internal.InputRangeMin)
                ChartData_Series_0 = new float[simData_Internal.InputRangeMax - simData_Internal.InputRangeMin];


            for (int i = simData_Internal.InputRangeMin; i < simData_Internal.InputRangeMax; i++)
            {
                ChartData_Series_0[i - simData_Internal.InputRangeMin] = bvc.CalculateValue(i);
            }

            ChartData_Series[0] = ChartData_Series_0;
        }


        // GUI


        protected override void EditorRelinkRootUIToolkit()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            var splitView = new TwoPaneSplitView(0, 700, TwoPaneSplitViewOrientation.Horizontal);

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
                // var property = serObj.FindProperty(nameof(simData));

                // EditorGUILayout.PropertyField(property, true);

                using (var changed = new EditorGUI.ChangeCheckScope())
                {
                    // This is where you draw the charts. PropertyField won't work here because `HideIf`
                    // Change it to your own drawing logic
                    GuiCharts();
                    // EditorGUILayout.PropertyField(property, true);
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

            // --- Draw the chart
            EditorGUI.DrawRect(chartRect, Color.blue);
//             _chartDrawer.DrawChart(
//                 chartRect, // Area where the chart is rendered
//                 ChartData_Series, // Y-axis data
//                 "Index (X)", // X-axis label
//                 i => $"#{simData_Internal.InputRangeMin + i}", // X-axis value formatter
//                 v => v.ToString("F1"), // Y-axis value formatter (1decimal)
//                 (s, idx) => new[] { $"Value: {s[idx]:F1}" }, // Tooltipcontent
//                 ChartBuilder.Scripts.Editor.ChartDrawerXY.ChartType.Line,
//
// // Chart type
//                 ChartData_Names, // Names of each data series
//                 ChartData_Colors, // Custom colors for the series
//                 8, // Number of X-axis grid lines
//                 7, // Number of Y-axis grid lines,
//                 i => $"# {simData_Internal.InputRangeMin + i}"
//             );
            GUILayout.EndVertical();
        }
    }
}
#endif
