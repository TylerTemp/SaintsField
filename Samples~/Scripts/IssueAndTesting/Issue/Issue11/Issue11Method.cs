using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue11
{
    public class Issue11Method : MonoBehaviour
    {
        [DisableIf(nameof(IsDisabledByMethod))] public string methodChecker;

        public bool IsDisabledByMethod()
        {
            // add your logic here
            return true;
        }

        // change your getter accordingly. This is just an example
        public bool IsDisabledByProperty
        {
            get => true;
            private set {}
        }

        [DisableIf(nameof(IsDisabledByProperty))] public string propertyChecker;

        public bool showMyMethod;

        #region  example of using property value
        [Button, PlayaShowIf(nameof(showMyMethod))]
        public void MyMethod1()
        {
            Debug.Log("Method1");
        }
        #endregion

        #region example of using method callback
#if UNITY_EDITOR  // this macro is not required, but in case you want to call some editor only methods
        public bool ShouldShowMyMethod()
        {
            // write your logic here
            return showMyMethod;
        }
#endif

#if UNITY_EDITOR
        [Button, PlayaShowIf(nameof(ShouldShowMyMethod))]
#endif
        public void MyMethod2()
        {
            Debug.Log("Method2");
        }
        #endregion

        // this works on normal field too
        [PlayaShowIf(nameof(showMyMethod))]
        public int myInt;
    }
}
