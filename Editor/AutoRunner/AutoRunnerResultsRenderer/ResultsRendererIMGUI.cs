using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer
    {

        protected override void RenderTargetIMGUI(PreCheckResult preCheckResult)
        {
            foreach ((MainTarget mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResult>> subGroup) in FormatResults(_autoRunner.results))
            {

                string groupLabel;
                if (!mainTarget.MainTargetIsAssetPath)
                {
                    groupLabel = mainTarget.MainTargetString;
                }
                else
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(mainTarget.MainTargetString);
                    if (obj == null)
                    {
                        continue;
                    }
                    EditorGUILayout.ObjectField(obj, typeof(Object), true);
                    groupLabel = obj.name;
                }
                EditorGUILayout.LabelField(groupLabel);

                using(new EditorGUI.IndentLevelScope(1))
                {
                    foreach (IGrouping<Object, AutoRunnerResult> grouping in subGroup)
                    {
                        if (grouping.Key == null)
                        {
                            continue;
                        }

                        // EditorGUILayout.LabelField(grouping.Key.name);
                        EditorGUILayout.ObjectField(grouping.Key, typeof(Object), true);

                        using(new EditorGUI.IndentLevelScope(1))
                        {
                            foreach (AutoRunnerResult autoRunnerResult in grouping)
                            {
                                EditorGUILayout.TextField("Field/Property", autoRunnerResult.propertyPath);

                                if (autoRunnerResult.FixerResult.ExecError != "")
                                {
                                    EditorGUILayout.HelpBox(autoRunnerResult.FixerResult.ExecError,
                                        MessageType.Warning);
                                }

                                if (autoRunnerResult.FixerResult.Error != "")
                                {
                                    EditorGUILayout.HelpBox(autoRunnerResult.FixerResult.Error,
                                        MessageType.Error);
                                }

                                // ReSharper disable once InvertIf
                                if (autoRunnerResult.FixerResult.CanFix)
                                {
                                    if (GUILayout.Button("Fix"))
                                    {
                                        autoRunnerResult.FixerResult.Callback();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
