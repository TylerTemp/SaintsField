#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SaintsField.SaintsXPathParser.XPathAttribute;
using UnityEngine;

// ReSharper disable ReplaceSubstringWithRangeIndexer


namespace SaintsField.SaintsXPathParser
{
    public static class XPathParser
    {
        public static IEnumerable<XPathStep> Parse(string xPath)
        {
            return PathParser.Parse(xPath);
        }

        internal static (Axis axis, string leftStepPart) SplitAxisFromStep(string step)
        {
            if (step.StartsWith("gameObject::"))
            {
                return (Axis.GameObject, step.Substring("gameObject::".Length));
            }
            if (step.StartsWith("field::"))
            {
                return (Axis.Field, step.Substring("field::".Length));
            }
            if (step.StartsWith("serialized-property::"))
            {
                return (Axis.SerializedProperty, step.Substring("serialized-property::".Length));
            }
            if(step.StartsWith("ancestor::"))
            {
                return (Axis.Ancestor, step.Substring("ancestor::".Length));
            }
            if(step.StartsWith("ancestor-inside-prefab::"))
            {
                return (Axis.AncestorInsidePrefab, step.Substring("ancestor-inside-prefab::".Length));
            }
            if(step.StartsWith("ancestor-or-self::"))
            {
                return (Axis.AncestorOrSelf, step.Substring("ancestor-or-self::".Length));
            }
            if(step.StartsWith("ancestor-or-self-inside-prefab::"))
            {
                return (Axis.AncestorOrSelfInsidePrefab, step.Substring("ancestor-or-self-inside-prefab::".Length));
            }
            if(step.StartsWith("parent::"))
            {
                return (Axis.Parent, step.Substring("parent::".Length));
            }
            if(step.StartsWith("parent-or-self::"))
            {
                return (Axis.ParentOrSelf, step.Substring("parent-or-self::".Length));
            }
            if(step.StartsWith("parent-or-self-inside-prefab::"))
            {
                return (Axis.ParentOrSelfInsidePrefab, step.Substring("parent-or-self-inside-prefab::".Length));
            }
            if(step.StartsWith("scene::"))
            {
                return (Axis.Scene, step.Substring("scene::".Length));
            }
            if(step.StartsWith("prefab::"))
            {
                return (Axis.Prefab, step.Substring("prefab::".Length));
            }
            if(step.StartsWith("resources::"))
            {
                return (Axis.Resources, step.Substring("resources::".Length));
            }
            if(step.StartsWith("resource::"))
            {
                return (Axis.Resources, step.Substring("resource::".Length));
            }
            if(step.StartsWith("assets::"))
            {
                return (Axis.Asset, step.Substring("assets::".Length));
            }
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(step.StartsWith("asset::"))
            {
                return (Axis.Asset, step.Substring("asset::".Length));
            }
            int axisSeparator = step.IndexOf("::", StringComparison.Ordinal);
            int attributeStart = step.IndexOf('@');
            if (axisSeparator >= 0 && (attributeStart < 0 || axisSeparator < attributeStart))
            {
                throw new ArgumentOutOfRangeException(nameof(step), step, "Unknown path axis");
            }
            return (Axis.None, step);
        }

        internal static NodeTest ParseNodeTest(string axisNameRaw)
        {
            switch (axisNameRaw)
            {
                case "":
                    return new NodeTest
                    {
                        NameEmpty = true,
                    };
                case "*":
                    return new NodeTest
                    {
                        NameAny = true,
                    };
            }

            List<string> rawFragments = axisNameRaw.Split('*').ToList();
            if (rawFragments.Count == 1)
            {
                return new NodeTest
                {
                    ExactMatch = axisNameRaw,
                };
            }

            bool startsWithFragment = !axisNameRaw.StartsWith("*");

            string startsWithStr = null;
            if (startsWithFragment)
            {
                startsWithStr = rawFragments[0];
                rawFragments.RemoveAt(0);
            }

            if(rawFragments.Count == 0)
            {
                return new NodeTest
                {
                    StartsWith = startsWithStr,
                };
            }

            int lastIndex = rawFragments.Count - 1;
            string endsLeftValue = rawFragments[lastIndex];
            rawFragments.RemoveAt(lastIndex);
            bool endsWithFragment = !endsLeftValue.EndsWith("*");

            return new NodeTest
            {
                StartsWith = startsWithStr,
                EndsWith = endsWithFragment? endsLeftValue: null,
                Contains = rawFragments,
            };
        }
    }
}

// ReSharper enable ReplaceSubstringWithRangeIndexer
#endif
