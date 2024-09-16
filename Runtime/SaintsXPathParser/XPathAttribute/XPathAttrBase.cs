using System;
using UnityEngine;

namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public abstract class XPathAttrBase
    {
        public static XPathAttrBase Parser(string attrString)
        {
            if (attrString == "last()")
            {
                return new XPathAttrIndex();
            }

            if (attrString == "index()")
            {
                return new XPathAttrIndex();
            }

            Debug.Assert(attrString.StartsWith('@'));
            // ReSharper disable once ReplaceSubstringWithRangeIndexer
            string attrTrim = attrString.Substring(1);
            if(attrTrim.StartsWith('{'))
            {
                Debug.Assert(attrTrim.EndsWith('}'), attrString);
                string evalString = attrTrim.Substring(1, attrTrim.Length - 2);
                return new XPathAttrFakeEval(evalString);
            }

            if (attrTrim == "layer")
            {
                return new XPathAttrLayer();
            }

            throw new ArgumentOutOfRangeException(nameof(attrString), attrString, null);
        }

    }
}
