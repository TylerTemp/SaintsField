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
        public static ToggleCheckInfo FillResult(ToggleCheckInfo toggleCheckInfo)
        {
            (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) = Util.ConditionChecker(toggleCheckInfo.ConditionInfos, null, null, toggleCheckInfo.Target);

            return new ToggleCheckInfo(toggleCheckInfo, errors, boolResults);
        }

        public static (bool show, bool disable) GetToggleResult(List<ToggleCheckInfo> toggleCheckInfos)
        {
            if (!toggleCheckInfos.TrueForAll((each) => each.Errors.Count == 0))
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

            foreach (ToggleCheckInfo toggleCheckInfo in toggleCheckInfos)
            {
                if (toggleCheckInfo.Errors.Count != 0)
                {
                    continue;
                }

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

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_DISABLE_ENABLE
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
