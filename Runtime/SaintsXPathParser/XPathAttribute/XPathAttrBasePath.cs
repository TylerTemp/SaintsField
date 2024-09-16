using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.SaintsXPathParser.XPathFilter;

namespace SaintsField.SaintsXPathParser.XPathAttribute
{
    public abstract class XPathAttrBasePath: XPathAttrBase
    {
        public struct PathFragment
        {
            public string StartsWith;
            public string EndsWith;
            public IReadOnlyList<string> Contains;
            public bool NameAny;
            public string ExactMatch;

            public bool Descendant;  // step starts with `//`

            // indexer, e.g [-1], [index() > 1], [last()]
            public FilterComparerInt IndexComparer;
        }

        public IReadOnlyList<PathFragment> PathFragments;

        protected XPathAttrBasePath(string basePath)
        {
            List<PathFragment> pathFragments = new List<PathFragment>();
            bool nextIsDescendant = false;
            foreach (string eachPath in basePath.Replace("\\", "/").Split('/'))
            {
                if (eachPath == "")
                {
                    nextIsDescendant = true;
                    continue;
                }

                bool thisIsDescendant = nextIsDescendant;
                nextIsDescendant = false;

                if (eachPath == "*")
                {
                    pathFragments.Add(new PathFragment
                    {
                        Contains = Array.Empty<string>(),
                        NameAny = true,
                        Descendant = thisIsDescendant,
                    });
                    continue;
                }

                List<string> rawFragments = eachPath.Split('*').ToList();
                if (rawFragments.Count == 1)
                {
                    (FilterComparerInt indexComparer, string leftValue) = ParseIndexFilter(eachPath);
                    pathFragments.Add(new PathFragment
                    {
                        Contains = Array.Empty<string>(),
                        ExactMatch = leftValue,
                        Descendant = thisIsDescendant,

                        IndexComparer = indexComparer,
                    });
                    continue;
                }

                bool startsWithFragment = !eachPath.StartsWith("*");

                string startsWithStr = null;
                if (startsWithFragment)
                {
                    startsWithStr = rawFragments[0];
                    rawFragments.RemoveAt(0);
                }

                if(rawFragments.Count == 0)
                {
                    (FilterComparerInt indexComparer, string leftValue) = ParseIndexFilter(startsWithStr);
                    pathFragments.Add(new PathFragment
                    {
                        StartsWith = leftValue,
                        Contains = Array.Empty<string>(),
                        Descendant = thisIsDescendant,

                        IndexComparer = indexComparer,
                    });
                    continue;
                }

                int lastIndex = rawFragments.Count - 1;
                (FilterComparerInt endsIndexComparer, string endsLeftValue) = ParseIndexFilter(rawFragments[lastIndex]);
                rawFragments.RemoveAt(lastIndex);

                bool endsWithFragment = !endsLeftValue.EndsWith("*");

                PathFragment pathFragment = new PathFragment
                {
                    StartsWith = startsWithStr,
                    EndsWith = endsWithFragment? endsLeftValue: null,
                    Contains = rawFragments,
                    Descendant = thisIsDescendant,

                    IndexComparer = endsIndexComparer,
                };

                pathFragments.Add(pathFragment);
            }

            PathFragments = pathFragments;
        }

        private (FilterComparerInt indexComparer, string leftValue) ParseIndexFilter(string part)
        {
            string[] split = part.Split('[', 2);
            if (split.Length == 1 || !split[1].EndsWith("]"))
            {
                return (null, part);
            }

            string indexComparerStr = split[1].Substring(0, split[1].Length - 1).Trim();

            return ((FilterComparerInt)FilterComparerBase.Parser(split[1]), split[0]);
        }
    }
}
