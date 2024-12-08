using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Condition;
using SaintsField.Editor.Utils;
using UnityEngine;

namespace SaintsField.Editor.Playa.Utils
{
    public static class SaintsEditorUtils
    {
        // public static (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) ConditionChecker(IEnumerable<ConditionInfo> conditionInfos, object target)
        // {
        //     List<bool> callbackBoolResults = new List<bool>();
        //     List<string> errors = new List<string>();
        //
        //     foreach (ConditionInfo conditionInfo in conditionInfos)
        //     {
        //         // ReSharper disable once UseNegatedPatternInIsExpression
        //         if (!(conditionInfo.Target is string conditionStringTarget))
        //         {
        //             Debug.Assert(conditionInfo.Compare == LogicCompare.Truly, $"target {conditionInfo.Target} should be truly compared");
        //             bool thisTruly = ReflectUtils.Truly(conditionInfo.Target);
        //             callbackBoolResults.Add(conditionInfo.Reverse ? !thisTruly : thisTruly);
        //             continue;
        //         }
        //
        //         (string error, object result) = Util.GetOfNoParams<object>(target, conditionStringTarget, null);
        //         if (error != "")
        //         {
        //             errors.Add(error);
        //             continue;
        //         }
        //
        //         object value = conditionInfo.Value;
        //         if (conditionInfo.ValueIsCallback)
        //         {
        //             Debug.Assert(value is string, $"value {value} of target {conditionInfo.Target} is not a string as a callback name");
        //             (string errorValue, object callbackResult) = Util.GetOfNoParams<object>(target, (string)value, null);
        //             if (errorValue != "")
        //             {
        //                 errors.Add(errorValue);
        //                 continue;
        //             }
        //
        //             value = callbackResult;
        //         }
        //
        //         bool boolResult;
        //         switch (conditionInfo.Compare)
        //         {
        //             case LogicCompare.Truly:
        //                 boolResult = ReflectUtils.Truly(result);
        //                 break;
        //             case LogicCompare.Equal:
        //                 boolResult = Util.GetIsEqual(result, value);
        //                 break;
        //             case LogicCompare.NotEqual:
        //                 boolResult = !Util.GetIsEqual(result, value);
        //                 break;
        //             case LogicCompare.GreaterThan:
        //                 boolResult = ((IComparable)result).CompareTo((IComparable)value) > 0;
        //                 break;
        //             case LogicCompare.LessThan:
        //                 boolResult = ((IComparable)result).CompareTo((IComparable)value) < 0;
        //                 break;
        //             case LogicCompare.GreaterEqual:
        //                 boolResult = ((IComparable)result).CompareTo((IComparable)value) >= 0;
        //                 break;
        //             case LogicCompare.LessEqual:
        //                 boolResult = ((IComparable)result).CompareTo((IComparable)value) <= 0;
        //                 break;
        //             case LogicCompare.BitAnd:
        //                 boolResult = ((int)result & (int)value) != 0;
        //                 break;
        //             case LogicCompare.BitXor:
        //                 boolResult = ((int)result ^ (int)value) != 0;
        //                 break;
        //             case LogicCompare.BitHasFlag:
        //             {
        //                 int valueInt = (int)value;
        //                 boolResult = ((int)result & valueInt) == valueInt;
        //             }
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException(nameof(conditionInfo.Compare), conditionInfo.Compare, null);
        //         }
        //         callbackBoolResults.Add(conditionInfo.Reverse ? !boolResult : boolResult);
        //     }
        //
        //     if (errors.Count > 0)
        //     {
        //         return (errors, Array.Empty<bool>());
        //     }
        //
        //     return (Array.Empty<string>(), callbackBoolResults);
        // }

        public static void FillResult(ToggleCheckInfo toggleCheckInfo)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(toggleCheckInfo.ConditionInfos, null, null, toggleCheckInfo.Target);

            if (errors.Count > 0)
            {
                toggleCheckInfo.Errors = errors;
                toggleCheckInfo.BoolResults = Array.Empty<bool>();
                return;
            }

            toggleCheckInfo.Errors = Array.Empty<string>();
            toggleCheckInfo.BoolResults = boolResults;
        }

        public static (bool show, bool disable) GetToggleResult(IReadOnlyList<ToggleCheckInfo> toggleCheckInfos)
        {
            if (toggleCheckInfos.Any(each => each.Errors.Count > 0))
            {
                return (true, false);
            }

            List<bool> showResults = new List<bool>();
            // bool hide = false;
            // no disable attribute: not-disable
            // any disable attribute is true: disable; otherwise: not-disable
            bool disable = false;
            // no enable attribute: enable
            // any enable attribute is true: enable; otherwise: not-enable
            bool enable = true;

            foreach (ToggleCheckInfo toggleCheckInfo in toggleCheckInfos.Where(each => each.Errors.Count == 0))
            {
                switch (toggleCheckInfo.Type)
                {
                    case ToggleType.Show:
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
                        Debug.Log(
                            $"show, count={toggleCheckInfo.boolResults.Count}, values={string.Join(",", toggleCheckInfo.boolResults)}");
#endif
                        showResults.Add(toggleCheckInfo.BoolResults.All(each => each));
                    }
                        break;
                    case ToggleType.Hide:
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
                        Debug.Log(
                            $"hide, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif

                        // Any(empty)=false=!hide=show. But because in ShowIf, empty=true=show, so we need to negate it.
                        if (toggleCheckInfo.BoolResults.Count == 0)
                        {
                            showResults.Add(false);  // don't show
                        }
                        else
                        {
                            bool willHide = toggleCheckInfo.BoolResults.Any(each => each);
                            showResults.Add(!willHide);
                        }
                    }
                        break;
                    case ToggleType.Disable:
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
                        Debug.Log(
                            $"disable, count={preCheckInternalInfo.boolResults.Count}, values={string.Join(",", preCheckInternalInfo.boolResults)}");
#endif
                        if (toggleCheckInfo.BoolResults.All(each => each))
                        {
                            disable = true;
                        }
                        break;
                    case ToggleType.Enable:
                        if (toggleCheckInfo.BoolResults.Count == 0)
                        {
                            // nothing means enable it or ignore
                        }
                        else
                        {
                            if (!toggleCheckInfo.BoolResults.Any(each => each))
                            {
                                enable = false;
                            }
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE || true
                        Debug.Log(
                            $"enable={enable}, count={toggleCheckInfo.BoolResults.Count}, values={string.Join(",", toggleCheckInfo.BoolResults)}");
#endif
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(toggleCheckInfo.Type), toggleCheckInfo.Type, null);
                }
            }

            bool showIfResult = showResults.Count == 0 || showResults.Any(each => each);
            bool disableIfResult = disable || !enable;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SHOW_HIDE
            Debug.Log(
                $"showIfResult={showIfResult} (hasShow={hasShow}, show={show}, hide={hide})");
#endif
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
            Debug.Log(
                $"disableIfResult={disableIfResult} (disable={disable}, enable={enable})");
#endif

            return (showIfResult, disableIfResult);
        }
    }
}
