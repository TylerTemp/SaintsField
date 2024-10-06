using System;
using UnityEngine;

#if UNITY_EDITOR
namespace SaintsField.SaintsXPathParser
{
    public static class NodeTestMatch
    {
        public static bool NodeMatch(string resourceName, NodeTest nodeTest)
        {
            if (nodeTest.NameAny || nodeTest.NameEmpty)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"name matched any={nodeTest.NameAny} empty={nodeTest.NameEmpty} -> {resourceName}, return true");;
#endif
                return true;
            }

            if (!string.IsNullOrEmpty(nodeTest.ExactMatch))
            {
                if (resourceName == nodeTest.ExactMatch || nodeTest.ExactMatch == ".")
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"name matched {nodeTest.ExactMatch}, return true");;
#endif
                    return true;
                }
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                Debug.Log($"name matched {nodeTest.ExactMatch} failed with {resourceName}, return false");;
#endif
                return false;
            }

            string checkingName = resourceName;
            if (!string.IsNullOrEmpty(nodeTest.StartsWith))
            {
                if (!checkingName.StartsWith(nodeTest.StartsWith))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"name startsWith not match: {resourceName} -> {nodeTest.StartsWith}, false");
#endif
                    return false;
                }

                checkingName = checkingName.Substring(nodeTest.StartsWith.Length);
            }

            if (!string.IsNullOrEmpty(nodeTest.EndsWith))
            {
                if (!checkingName.EndsWith(nodeTest.EndsWith))
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                    Debug.Log($"name endsWith not match: {resourceName} -> {nodeTest.EndsWith}, false");
#endif
                    return false;
                }

                checkingName = checkingName.Substring(0, checkingName.Length - nodeTest.EndsWith.Length);
            }

            if (nodeTest.Contains != null)
            {
                foreach (string axisNameContain in nodeTest.Contains)
                {
                    int containIndex = checkingName.IndexOf(axisNameContain, StringComparison.Ordinal);
                    if (containIndex == -1)
                    {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
                        Debug.Log($"name contains not match: {axisNameContain} -> {checkingName}, false");
#endif
                        return false;
                    }
                    checkingName = checkingName.Substring(0, containIndex) + checkingName.Substring(containIndex + axisNameContain.Length);
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_PATH
            Debug.Log($"name matched as passed all conditions: {resourceName} -> {nodeTest}");
#endif
            return true;
        }
    }
}
#endif
