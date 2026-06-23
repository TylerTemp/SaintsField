#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer
    {
        private VisualElement _root;
        private IReadOnlyList<AutoRunnerResult> _results = new List<AutoRunnerResult>();

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            _root = new VisualElement();
            // Debug.Log(AutoRunner);
            // AutoRunnerResult[] results = AutoRunner.results;
            return (_root, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult preCheckResult = UpdatePreCheckUIToolkitInternal(FieldWithInfo, _root);

            // if(_autoRunner.results == null)
            // {
            //     _results = new List<AutoRunnerResult>();
            //     _root.Clear();
            //     return preCheckResult;
            // }

            if (_autoRunner.Results.SequenceEqual(_results))
            {
                return preCheckResult;
            }

            _results = _autoRunner.Results.ToArray();
            _root.Clear();

            if (_autoRunner.Results.Count == 0)
            {
                return preCheckResult;
            }

            (AutoRunnerResult value, int index)[] canFixWithIndex =
                GetFixableResultsWithIndex(_autoRunner.Results);
            if (canFixWithIndex.Length > 0)
            {
                _root.Add(new Button(() =>
                {
                    RunFixAllAndRemove(canFixWithIndex);
                    _results = Array.Empty<AutoRunnerResult>();
                    OnUpdateUIToolKit(root);
                })
                {
                    text = "Fix All",
                });
            }

            (object mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResultInfo>> subGroup)[] formatedResults = FormatResults(_autoRunner.Results).ToArray();

            if (formatedResults.Length == 0)
            {
                Debug.Log($"#AutoRunner# no targets");
                return preCheckResult;
            }

            foreach ((object mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResultInfo>> subGroup) in formatedResults)
            {
                // Debug.Log($"#AutoRunner# draw {mainTarget}");
                Foldout group = new Foldout
                {
                    // text = mainTarget as string ?? mainTarget.ToString(),
                };
                if (mainTarget is string mainTargetString)
                {
                    group.text = mainTargetString;
                }
                else
                {
                    Object obj = mainTarget as Object;
                    if (obj == null)
                    {
                        Debug.Log($"#AutoRunner# target is null: {mainTarget}");
                        continue;
                    }

                    ObjectField objField = new ObjectField
                    {
                        value = obj,
                    };
                    objField.AddToClassList(ObjectField.alignedFieldUssClassName);
                    group.Add(objField);
                    group.text = obj.name;
                }

                VisualElement subGroupElement = new VisualElement
                {
                    // style =
                    // {
                    //     paddingLeft = 4,
                    //     // backgroundColor = EColor.Aqua.GetColor(),
                    // },
                };
                foreach (IGrouping<Object, AutoRunnerResultInfo> grouping in subGroup)
                {
                    if (grouping.Key == null)
                    {
                        Debug.Log($"#AutoRunner# skip null group for {mainTarget}");
                        continue;
                    }

                    Foldout subGroupElementGroup = new Foldout
                    {
                        text = grouping.Key.name,
                    };
                    subGroupElementGroup.Add(new ObjectField
                    {
                        value = grouping.Key,
                    });
                    // Debug.Log($"#AutoRunner# draw {grouping.Key} for {mainTarget}");
                    foreach (AutoRunnerResultInfo autoRunnerResultInfo in grouping)
                    {
                        VisualElement subGroupElementGroupElement = new VisualElement();

                        TextField serializedPropertyLabel = new TextField("Field/Property")
                        {
                            value = autoRunnerResultInfo.AutoRunnerResult.propertyPath,
                        };
                        // serializedPropertyLabel.SetEnabled(false);
                        subGroupElementGroupElement.Add(serializedPropertyLabel);

                        if (!string.IsNullOrEmpty(autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError))
                        {
                            subGroupElementGroupElement.Add(new HelpBox(autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError, HelpBoxMessageType.Warning));
                        }

                        if (!string.IsNullOrEmpty(autoRunnerResultInfo.AutoRunnerResult.FixerResult.Error))
                        {
                            subGroupElementGroupElement.Add(new HelpBox(autoRunnerResultInfo.AutoRunnerResult.FixerResult.Error, HelpBoxMessageType.Error));
                        }

                        if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.CanFix)
                        {
                            subGroupElementGroupElement.Add(new Button(() =>
                            {
                                RunFixAndRemove(autoRunnerResultInfo);
                                _results = Array.Empty<AutoRunnerResult>();
                                OnUpdateUIToolKit(root);
                            })
                            {
                                text = "Fix",
                            });
                        }

                        subGroupElementGroup.Add(subGroupElementGroupElement);
                    }

                    subGroupElement.Add(subGroupElementGroup);
                }

                group.Add(subGroupElement);
                _root.Add(group);
            }

            return preCheckResult;
        }
    }
}
#endif
