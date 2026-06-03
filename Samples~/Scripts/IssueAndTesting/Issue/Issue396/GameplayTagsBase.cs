using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue396
{
    [DisallowMultipleComponent]
    public abstract class GameplayTagsBase : MonoBehaviour, ILevelBound
    {
        public void Construct()
        {
            // GameplayTagsManager.Instance.Register(this);
        }

        private void OnDestroy()
        {
            // GameplayTagsManager.Instance.Unregister(this);
        }
    }
}
