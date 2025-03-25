#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Linq;
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

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement container)
        {
            _root = new VisualElement();
            // Debug.Log(AutoRunner);
            // AutoRunnerResult[] results = AutoRunner.results;
            return (_root, true);
        }

        protected override PreCheckResult OnUpdateUIToolKit(VisualElement root)
        {
            PreCheckResult preCheckResult = HelperOnUpdateUIToolKitRawBase();

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

            (AutoRunnerResult value, int index)[] canFixWithIndex = _autoRunner.Results
                .WithIndex()
                .Where(each => each.value.FixerResult?.CanFix ?? false)
                .Reverse()
                .ToArray();
            if (canFixWithIndex.Length > 0)
            {
                _root.Add(new Button(() =>
                {
                    List<int> toRemoveIndex = new List<int>();
                    foreach ((AutoRunnerResult autoRunnerResult, int index) in canFixWithIndex)
                    {
                        bool errorFixed = false;
                        try
                        {
                            autoRunnerResult.FixerResult.Callback();
                            errorFixed = true;
                        }
                        catch (Exception e)
                        {
                            autoRunnerResult.FixerResult.ExecError = e.Message;
                        }

                        if (errorFixed)
                        {
                            toRemoveIndex.Add(index);
                        }
                    }

                    // ReSharper disable once InvertIf
                    if(toRemoveIndex.Count > 0)
                    {
                        foreach (int index in toRemoveIndex)
                        {
                            _autoRunner.Results.RemoveAt(index);
                        }

                        OnUpdateUIToolKit(root);
                    }
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

                        if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError != "")
                        {
                            subGroupElementGroupElement.Add(new HelpBox(autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError, HelpBoxMessageType.Warning));
                        }

                        if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.Error != "")
                        {
                            subGroupElementGroupElement.Add(new HelpBox(autoRunnerResultInfo.AutoRunnerResult.FixerResult.Error, HelpBoxMessageType.Error));
                        }

                        if (autoRunnerResultInfo.AutoRunnerResult.FixerResult.CanFix)
                        {
                            subGroupElementGroupElement.Add(new Button(() =>
                            {
                                try
                                {
                                    autoRunnerResultInfo.AutoRunnerResult.FixerResult.Callback();
                                }
                                catch (Exception e)
                                {
                                    autoRunnerResultInfo.AutoRunnerResult.FixerResult.ExecError = e.Message;
                                    OnUpdateUIToolKit(root);
                                    return;
                                }

                                _autoRunner.Results.RemoveAt(autoRunnerResultInfo.Index);
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
