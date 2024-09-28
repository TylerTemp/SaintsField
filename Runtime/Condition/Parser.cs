using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SaintsField.Condition
{
    public static class Parser
    {
        public static IEnumerable<ConditionInfo> Parse(IReadOnlyList<object> rawConditions)
        {
            int totalLength = rawConditions.Count;
            bool skipNext = false;
            for (int index = 0; index < totalLength; index++)
            {
                if (skipNext)
                {
                    skipNext = false;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CONDITION
                    Debug.Log($"#Condition# Skip {rawConditions[index]}");
#endif
                    continue;
                }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CONDITION
                Debug.Log($"#Condition# Get {rawConditions[index]}");
#endif

                object rawObjectCondition = rawConditions[index];
                if (!(rawObjectCondition is string rawCondition))
                {
                    yield return new ConditionInfo
                    {
                        Target = rawObjectCondition,
                        Compare = LogicCompare.Truly,
                        Value = null,
                        ValueIsCallback = false,
                        Reverse = false,
                    };
                    continue;
                }
                // string rawCondition = (string);

                bool reverse = false;
                object value = null;

                if(rawCondition.StartsWith("!"))
                {
                    reverse = true;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CONDITION
                    Debug.Log($"#Condition# {rawCondition} is reverse");
#endif
                    rawCondition = rawCondition.Substring(1);
                }
                else if (rawCondition.StartsWith("$"))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CONDITION
                    Debug.Log($"#Condition# {rawCondition} remove !");
#endif
                    rawCondition = rawCondition.Substring(1);
                }

                LogicCompare logicCompare = LogicCompare.Truly;
                bool valueIsCallback = false;

                // bits
                if (rawCondition.EndsWith("&"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 1);
                    logicCompare = LogicCompare.BitAnd;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("&$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.BitAnd;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("^"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 1);
                    logicCompare = LogicCompare.BitXor;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("^$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.BitXor;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("&=="))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 3);
                    logicCompare = LogicCompare.BitHasFlag;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("&==$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 4);
                    logicCompare = LogicCompare.BitHasFlag;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                // compares
                else if (rawCondition.EndsWith("=="))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.Equal;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("==$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 3);
                    logicCompare = LogicCompare.Equal;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("!="))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.NotEqual;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("!=$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 3);
                    logicCompare = LogicCompare.NotEqual;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith(">"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 1);
                    logicCompare = LogicCompare.GreaterThan;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith(">$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.GreaterThan;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith(">="))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.GreaterEqual;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith(">=$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 3);
                    logicCompare = LogicCompare.GreaterEqual;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if(rawCondition.EndsWith("<"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 1);
                    logicCompare = LogicCompare.LessThan;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("<$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.LessThan;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("<="))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 2);
                    logicCompare = LogicCompare.LessEqual;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("<=$"))
                {
                    skipNext = true;
                    rawCondition = rawCondition.Substring(0, rawCondition.Length - 3);
                    logicCompare = LogicCompare.LessEqual;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else
                {
                    bool hasNext = index + 1 < totalLength;
                    if (hasNext)
                    {
                        object nextValue = rawConditions[index + 1];
                        switch (nextValue)
                        {
                            case string _:
                                break;
                            case Enum enumValue:
                            {
                                skipNext = true;

                                bool isFlag = enumValue.GetType().GetCustomAttribute<FlagsAttribute>() != null;
                                logicCompare = isFlag
                                    ? LogicCompare.BitHasFlag
                                    : LogicCompare.Equal;

                                value = enumValue;
                            }
                                break;
                            default:
                            {
                                skipNext = true;

                                value = nextValue;
                                logicCompare = LogicCompare.Equal;
                            }
                                break;
                        }
                    }
                }

                yield return new ConditionInfo
                {
                    Target = rawCondition,
                    Compare = logicCompare,
                    Value = value,
                    ValueIsCallback = valueIsCallback,
                    Reverse = reverse,
                };
            }
        }
    }
}
