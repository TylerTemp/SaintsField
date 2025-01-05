using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.SaintsEditorWindowUtils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.AutoRunner
{
    public class AutoRunnerEditor: SaintsEditorWindowSpecialEditor
    {
#if UNITY_2021_3_OR_NEWER
        private class ResultsRenderer: SerializedFieldRenderer
        {
            private readonly AutoRunnerWindow _autoRunner;

            public ResultsRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
            {
                _autoRunner = (AutoRunnerWindow) serializedObject.targetObject;
            }

            private VisualElement _root;
            private AutoRunnerResult[] _results = {};

            protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
            {
                _root = new VisualElement();
                // Debug.Log(AutoRunner);
                // AutoRunnerResult[] results = AutoRunner.results;
                return (_root, true);
            }

            private struct MainTarget : IEquatable<MainTarget>
            {
                public string MainTargetString;
                public bool MainTargetIsAssetPath;

                public bool Equals(MainTarget other)
                {
                    return MainTargetString == other.MainTargetString && MainTargetIsAssetPath == other.MainTargetIsAssetPath;
                }

                public override bool Equals(object obj)
                {
                    return obj is MainTarget other && Equals(other);
                }

                public override int GetHashCode()
                {
                    return Util.CombineHashCode(MainTargetString, MainTargetIsAssetPath);
                }
            }

            protected override PreCheckResult OnUpdateUIToolKit()
                // private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition, bool richLabelCondition, FieldInfo info, object parent)
            {
                PreCheckResult preCheckResult = base.OnUpdateUIToolKit();

                if (_autoRunner.results.SequenceEqual(_results))
                {
                    return preCheckResult;
                }

                _results = _autoRunner.results;
                _root.Clear();

                foreach ((MainTarget mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResult>> subGroup) in _autoRunner.results
                             .GroupBy(each => new MainTarget
                             {
                                    MainTargetString = each.mainTargetString,
                                    MainTargetIsAssetPath = each.mainTargetIsAssetPath,
                             })
                             .Select(each => (
                                 each.Key,
                                 each.GroupBy(sub => sub.subTarget)
                            )))
                {
                    // Debug.Log($"#AutoRunner# draw {mainTarget}");
                    Foldout group = new Foldout
                    {
                        // text = mainTarget as string ?? mainTarget.ToString(),
                    };
                    if (!mainTarget.MainTargetIsAssetPath)
                    {
                        group.text = mainTarget.MainTargetString;
                    }
                    else
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(mainTarget.MainTargetString);
                        if (obj == null)
                        {
                            continue;
                        }
                        group.Add(new ObjectField
                        {
                            value = obj,
                        });
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
                    foreach (IGrouping<Object,AutoRunnerResult> grouping in subGroup)
                    {
                        if (grouping.Key == null)
                        {
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
                        foreach (AutoRunnerResult autoRunnerResult in grouping)
                        {
                            VisualElement subGroupElementGroupElement = new VisualElement();

                            TextField serializedPropertyLabel = new TextField("Field/Property")
                            {
                                value = autoRunnerResult.propertyPath,
                            };
                            // serializedPropertyLabel.SetEnabled(false);
                            subGroupElementGroupElement.Add(serializedPropertyLabel);

                            if (autoRunnerResult.FixerResult.ExecError != "")
                            {
                                subGroupElementGroupElement.Add(new HelpBox(autoRunnerResult.FixerResult.ExecError, HelpBoxMessageType.Warning));
                            }

                            if (autoRunnerResult.FixerResult.Error != "")
                            {
                                subGroupElementGroupElement.Add(new HelpBox(autoRunnerResult.FixerResult.Error, HelpBoxMessageType.Error));
                            }

                            if (autoRunnerResult.FixerResult.CanFix)
                            {
                                subGroupElementGroupElement.Add(new Button(() => autoRunnerResult.FixerResult.Callback())
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

        public override AbsRenderer MakeRenderer(SerializedObject so, SaintsFieldWithInfo fieldWithInfo)
        {
            if (fieldWithInfo.FieldInfo?.Name == "results")
            {
                return new ResultsRenderer(so, fieldWithInfo);
            }

            // Debug.Log($"{fieldWithInfo.RenderType}/{fieldWithInfo.FieldInfo?.Name}/{string.Join(",", fieldWithInfo.PlayaAttributes)}");
            return base.MakeRenderer(so, fieldWithInfo);
        }
#endif
    }
}
