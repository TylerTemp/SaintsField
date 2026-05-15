using System;
#if UNITY_EDITOR
using SaintsField.Editor.Utils;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue389
{
    [Serializable]
    public class BuildingAttackBrain
    {
        [EnableIf(nameof(EnableIfDamage))]
        [InfoBox("Only available in BuildObject type", show: nameof(ShowMessage))]
        public int damage;

        private bool ShowMessage() => !EnableIfDamage();

        private bool EnableIfDamage()
        {
#if UNITY_EDITOR
            // ReSharper disable once InvertIf
            if(SaintsContext.SerializedProperty != null)
            {
                (Attribute[] _, object parent) =
                    SerializedUtils.GetAttributesAndDirectParent<Attribute>(SaintsContext.FindPropertyRelateTo(".."));
                // Debug.Log(parent);
                if (parent is BuildObject)
                {
                    return true;
                }
            }
            return false;
#endif
        }
    }
}
