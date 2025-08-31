using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;
using MCDamageHandler = SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue270.MCDamageHandler;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue132
{
    public class Issue132Error : Issue132Base
    {
        public override  MCDamageHandler realSelf => parent;
        public override bool            isRelay  => true;

        [LayoutGroup(" ------ CHILD -----",ELayout.Title|ELayout.Background),
         GetComponentInParents(excludeSelf: true),
         HideIf(nameof(_parentOverride)),
        ]
        // ReSharper disable once InconsistentNaming
        public MCDamageHandler _parent;

        [Tooltip("set this to override the auto parent")]
        // ReSharper disable once InconsistentNaming
        public MCDamageHandler _parentOverride;
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public          MCDamageHandler parent => _parentOverride ? _parentOverride : _parent;
        public override MCTeam          team   => null;
        // 0 = totally immune to damage
        public float damageRatio = 1;
    }
}
