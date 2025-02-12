using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer
    {

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            foreach ((object mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResultInfo>> subGroup) in FormatResults(_autoRunner.Results))
            {

                string groupLabel;
                if (mainTarget is string mainTargetString)
                {
                    groupLabel = mainTargetString;
                }
                else
                {
                    Object obj = mainTarget as Object;
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
                    foreach (IGrouping<Object, AutoRunnerResultInfo> grouping in subGroup)
                    {
                        if (grouping.Key == null)
                        {
                            continue;
                        }

                        // EditorGUILayout.LabelField(grouping.Key.name);
                        EditorGUILayout.ObjectField(grouping.Key, typeof(Object), true);

                        using(new EditorGUI.IndentLevelScope(1))
                        {
                            foreach (AutoRunnerResultInfo autoRunnerResultInfo in grouping)
                            {
                                EditorGUILayout.TextField("Field/Property", autoRunnerResultInfo.AutoRunnerResult.propertyPath);

                                if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError != "")
                                {
                                    EditorGUILayout.HelpBox(autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError,
                                        MessageType.Warning);
                                }

                                if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.Error != "")
                                {
                                    EditorGUILayout.HelpBox(autoRunnerResultInfo.AutoRunnerResult.FixerResult.Error,
                                        MessageType.Error);
                                }

                                // ReSharper disable once InvertIf
                                if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.CanFix)
                                {
                                    if (GUILayout.Button("Fix"))
                                    {
                                        bool errorFixed = false;
                                        try
                                        {
                                            autoRunnerResultInfo.AutoRunnerResult.FixerResult.Callback();
                                            errorFixed = true;
                                        }
                                        catch (System.Exception e)
                                        {
                                            Debug.LogError(e);
                                            autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError = e.ToString();
                                        }

                                        if (errorFixed)
                                        {
                                            _autoRunner.Results.RemoveAt(autoRunnerResultInfo.Index);
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
}
