using System;
using System.Collections.Generic;
using System.Reflection;

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
                    continue;
                }

                string rawCondition = (string)rawConditions[index];

                bool reverse = false;

                if(rawCondition.StartsWith("!"))
                {
                    reverse = true;
                    rawCondition = rawCondition.Substring(1);
                }
                else if (rawCondition.StartsWith("$"))
                {
                    rawCondition = rawCondition.Substring(1);
                }

                LogicCompare logicCompare = LogicCompare.Truly;
                object value = null;
                bool valueIsCallback = false;
                if (rawCondition.EndsWith("=="))
                {
                    logicCompare = LogicCompare.Equal;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("==$"))
                {
                    logicCompare = LogicCompare.Equal;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("!="))
                {
                    logicCompare = LogicCompare.NotEqual;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("!=$"))
                {
                    logicCompare = LogicCompare.NotEqual;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith(">"))
                {
                    logicCompare = LogicCompare.GreaterThan;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith(">$"))
                {
                    logicCompare = LogicCompare.GreaterThan;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith(">="))
                {
                    logicCompare = LogicCompare.GreaterEqual;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith(">=$"))
                {
                    logicCompare = LogicCompare.GreaterEqual;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if(rawCondition.EndsWith("<"))
                {
                    logicCompare = LogicCompare.LessThan;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("<$"))
                {
                    logicCompare = LogicCompare.LessThan;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("<="))
                {
                    logicCompare = LogicCompare.LessEqual;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("<=$"))
                {
                    logicCompare = LogicCompare.LessEqual;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("&&"))
                {
                    logicCompare = LogicCompare.BitAnd;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("&&$"))
                {
                    logicCompare = LogicCompare.BitAnd;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("^"))
                {
                    logicCompare = LogicCompare.BitXor;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("^$"))
                {
                    logicCompare = LogicCompare.BitXor;
                    value = rawConditions[index + 1];
                    valueIsCallback = true;
                }
                else if (rawCondition.EndsWith("&=="))
                {
                    logicCompare = LogicCompare.BitHasFlag;
                    value = rawConditions[index + 1];
                }
                else if (rawCondition.EndsWith("&==$"))
                {
                    logicCompare = LogicCompare.BitHasFlag;
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
                                bool isFlag = enumValue.GetType().GetCustomAttribute<FlagsAttribute>() != null;
                                logicCompare = isFlag
                                    ? LogicCompare.BitHasFlag
                                    : LogicCompare.Equal;

                                value = enumValue;
                                skipNext = true;
                            }
                                break;
                            default:
                            {
                                value = nextValue;
                                logicCompare = LogicCompare.Equal;
                                skipNext = true;
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

                if (valueIsCallback)
                {
                    skipNext = true;
                }

            }
        }
    }
}
