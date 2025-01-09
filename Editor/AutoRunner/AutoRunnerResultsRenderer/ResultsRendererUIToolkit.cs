#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SaintsField.Editor.AutoRunner.AutoRunnerResultsRenderer
{
    public partial class ResultsRenderer
    {
        private VisualElement _root;
        private IReadOnlyList<AutoRunnerResult> _results = new List<AutoRunnerResult>();

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit()
        {
            _root = new VisualElement();
            // Debug.Log(AutoRunner);
            // AutoRunnerResult[] results = AutoRunner.results;
            return (_root, true);
        }


        protected override PreCheckResult OnUpdateUIToolKit()
            // private void UIToolkitCheckUpdate(VisualElement result, bool ifCondition, bool arraySizeCondition, bool richLabelCondition, FieldInfo info, object parent)
        {
            PreCheckResult preCheckResult = base.OnUpdateUIToolKit();

            if (_autoRunner.results.SequenceEqual(_results))
            {
                return preCheckResult;
            }

            _results = _autoRunner.results.ToArray();
            _root.Clear();

            foreach ((MainTarget mainTarget, IEnumerable<IGrouping<Object, AutoRunnerResult>> subGroup) in FormatResults(_autoRunner.results))
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
}
#endif
