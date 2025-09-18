using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue297 : SaintsMonoBehaviour
    {
        public bool isRelay;

        [Expandable,HideIf(nameof(isRelay)), SerializeField] internal MCTeam          _team;
        // public virtual                                               MCDamageHandler realSelf => this;
        // [Expandable,Required,HideIf(nameof(isRelay))] public         FloatValue_SO   healthMultiply_SO;
        // [Expandable,HideIf(nameof(isRelay))]          public         FloatValue_SO   regenAdd_SO;
        // [SerializeField,HideIf(nameof(isRelay))]
        // float  _initialHealth = 100f
        //             ,_regenSpeed;
        // internal                                     float InitialHealth => _initialHealth*healthMultiply_SO.TotalValue;
        // [HideIf(nameof(isRelay)),HeaderLabel] public float _currentHealth;
        // [FormerlySerializedAs("_head")]
        // [HideIf(nameof(isRelay)),SerializeField]
        // Transform  head
        //                     ,body;
        // [GetComponent,HideIf(nameof(isRelay))] public PandaBehaviour        panda;
        // [GetComponent,HideIf(nameof(isRelay))] public MCAnimationController anim;
        // public                                     Transform             Head => body ? body : head ?  head :transform; //we prioritize body over head to help the navmesh
        // public Transform GetHeadOrBody() => Head;
        // public virtual MCTeam team => _team;
        // public void SetTeam(MCTeam team) => _team = team;
        // [ShowInInspector]
        // public virtual float Health
        // {
        //     get => _currentHealth;
        //     set {
        //         if(!enabled) return;
        //         if(value>=InitialHealth || value==_currentHealth) {
        //             value = InitialHealth;
        //             return;
        //         }
        //         // still has armors? we blink it and its armors then skip damage processing
        //         if(IsInvulnerable) {
        //             BlinkMaterials(damage: true,hacking: false,invulnerable: true,isShield: false,heal: false);
        //             armors.ForEach(ca => ca.self.BlinkMaterials(damage: false,hacking: false,invulnerable: false,isShield: true,heal: false));
        //             return;
        //         }
        //         // if unarmored we process the damage
        //         if(value<_currentHealth) {
        //             BlinkMaterials(damage: true,false,false,false,false);
        //             if(!panda && value<0.01f) { Die(); } // if there is a panda, let it deal with MEGAdeath
        //         }
        //         if(value>_currentHealth) BlinkMaterials(damage: true,false,false,false,true);
        //         _currentHealth = value;
        //         onNormalizedHealthChange.Invoke(normalizedHealth);
        //     }
        // }
    }
}
