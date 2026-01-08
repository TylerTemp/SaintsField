using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SaintsField.Editor
{
    public class MemberInfoComparerReflection : IComparer<MemberInfo>, IComparer
    {
        public int Compare(MemberInfo x, MemberInfo y)
        {
            Debug.Assert(x != null);
            Debug.Assert(y != null);
            return x.MetadataToken - y.MetadataToken;
        }

        public int Compare(object x, object y)
        {
            if (x is MemberInfo xM && y is MemberInfo yM)
            {
                return Compare(xM, yM);
            }

            return 0;
        }
    }
}
