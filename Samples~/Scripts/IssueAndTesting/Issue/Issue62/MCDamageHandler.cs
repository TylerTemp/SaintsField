using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue79;
using SaintsField.Samples.Scripts.IssueAndTesting.Issue46;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue62
{
    public class MCDamageHandler : MonoBehaviour
    {

		public virtual MCDamageHandler realSelf => this;
		[SerializeField,HideIf(nameof(isRelay))]
		internal float _initialHealth = 100f;
		[HideIf(nameof(isRelay))]
		public float _currentHealth;
		[HideIf(nameof(isRelay))]
		public Transform _head;
		[GetComponent,HideIf(nameof(isRelay))]
		public PandaBehaviour panda;
		[GetComponent,HideIf(nameof(isRelay))]
		public MCAnimationController anim;
		public         Transform head => _head==null ? transform : _head;
		public virtual MCTeam    team => _team;
		[Expandable]
		[HideIf(nameof(isRelay))]
		[SerializeField]
		internal MCTeam _team;
		[ShowInInspector]
		public virtual float Health
		{
			get => _currentHealth;
			set {
				if(!enabled) return;
				// if(_selfArmor && _currentHealth-value<1) { //armors only lose health if damage >=1
				// 	onNormalizedHealthChange.Invoke(normalizedHealth);
				// 	BlinkMaterials(hacking: false,invulnerable: true);
				// 	return;
				// }
				// if(childrenArmors.Count>0) { //parents with armors blink their armors
				// 	childrenArmors.RemoveAll(c => c==null);
				// 	childrenArmors.ForEach(ca => ca.self.BlinkMaterials(hacking: false,invulnerable: true));
				// 	return;
				// }
				if(value<_currentHealth) {
					BlinkMaterials(false,false);
					if(!panda && value<0.01f) { Die(); } // if there is a panda, let it deal with MEGAdeath
				}
				_currentHealth = value;
				// onNormalizedHealthChange.Invoke(normalizedHealth);
			}
		}
		public float normalizedHealth => _currentHealth/_initialHealth;
		[HideIf(nameof(isRelay))]
		public Transform targetOffset;
		[SepTitle("DAMAGE",EColor.Brown)]
		[HideIf(nameof(isRelay))]
		public bool kickback = true;
		// [GetComponent,ReadOnly,EnableIf(nameof(kickback)),HideIf(nameof(isRelay))]
		// public NavMeshAgent agent;
		[GetComponent,ReadOnly,EnableIf(nameof(kickback)),HideIf(nameof(isRelay))]
		public Rigidbody rigid;
		// [GetComponent,ReadOnly]
		// public MCArmor _selfArmor;
		// [HideIf(nameof(isRelay))]
		// public List<MCArmor> childrenArmors;
		public List<Renderer> blinkRenderers;
		// [HideIf(nameof(isRelay))]
		// public UltEvent<float> onNormalizedHealthChange;
		[SepTitle("DEATH",EColor.Black)]
		[HideIf(nameof(isRelay))]
		public bool destroyOnDeath = true;
		[EnableIf(nameof(destroyOnDeath)),HideIf(nameof(isRelay))]
		public float deathDelay = 1f;
		[Tooltip("can be instantiated even when destroy is off, usable as a jetisson FX")]
		[HideIf(nameof(isRelay))]
		public GameObject explosionPrefab,
											corpsePrefab;
		// [HideIf(nameof(isRelay))]
		// public UltEvent onDeath;
		// [LayoutEnd("Death")]
		MaterialPropertyBlock _block;
		static readonly int   _blinkTime    = Shader.PropertyToID("_blinkTime");
		static readonly int   _hacking      = Shader.PropertyToID("_hacking");
		static readonly int   _invulnerable = Shader.PropertyToID("_invulnerable");
		static readonly int   _command      = Shader.PropertyToID("_command");
		public virtual void OnEnable() => Health = _currentHealth = _initialHealth;

		public void BlinkMaterials(bool hacking,bool invulnerable)
		{
			blinkRenderers.RemoveAll(r => r==null);
			if(_block==null) _block = new MaterialPropertyBlock();
			blinkRenderers.ForEach(r =>
														{
															r.GetPropertyBlock(_block);
															_block.SetFloat(_blinkTime,Time.timeSinceLevelLoad);
															_block.SetFloat(_hacking,hacking ? 1 : 0);
															_block.SetFloat(_invulnerable,invulnerable ? 1 : 0);
															r.SetPropertyBlock(_block);
														});
		}

		// [PandaTask]
		public bool IsHealthLessThan(float h) => _currentHealth<h;

		// [PandaTask]
		public bool IsHealthNormalizedLessThan(float hn) => normalizedHealth<hn;

		// [PandaTask]
		public bool IsHealth_PercentLessThan(float percent) => normalizedHealth*100.0<percent;

		// [PandaTask]
		public bool IsDead => Health<0.1f;
		public virtual bool isRelay => false;

		public virtual float TakeDamage(float damage)
		{
			Health -= damage;
			// if(damage<0) anim.PlayAnimationAndSound(ANIMSTATE.hit);
			if(_currentHealth<=0 && !panda) { Die(); }
			return damage;
		}

		// public virtual float HandleDamageProvider(HVRDamageProvider damageProvider,float damage,Vector3 hitPoint,Vector3 direction)
		// {
		// 	if(rigid) { rigid.AddForceAtPosition(damage*damageProvider.forceMultiplier*direction,hitPoint,ForceMode.Impulse); }
		// 	return TakeDamage(damage);
		// }

		// [PandaTask]
		public virtual void BeforeDie()
		{
			// if(!anim || !anim.PlayAnimationAndSound(ANIMSTATE.die)) PandaTask.Succeed();
		}

		// [PandaTask]
		public virtual void Die()
		{
			// if(!panda || PandaTask.isStarting) {
			// 	// if(panda) PandaTask.data = Time.time+deathDelay;
			// 	DoDie();
			// }
			// if(panda) {
			// 	PandaTask.Succeed();
			// 	panda.enabled = false;
			// }
		}

		public void DoDie()
		{
			// if(explosionPrefab) {
			// 	var explosion = Pool.Instantiate(explosionPrefab,head.position,Quaternion.identity);
			// 	if(explosion.TryGetComponent<MCExplosion>(out var exploScript)) {
			// 		exploScript.team       = team;
			// 		exploScript.originator = realSelf;
			// 	}
			// }
			// if(corpsePrefab) Pool.Instantiate(corpsePrefab,transform.position,transform.rotation);
			// if(destroyOnDeath) { Destroy(gameObject,deathDelay); }
			// if(_selfArmor) _selfArmor.Detach();
			// if(rigid && !rigid.isKinematic) rigid.AddForce(2*(transform.up+0.5f*Random.onUnitSphere),ForceMode.Impulse);
			// onDeath.Invoke();
			// SendMessage("OnDeath",head.position,SendMessageOptions.DontRequireReceiver);
		}

		// public virtual void HandleRayCastHit(HVRDamageProvider damageProvider,RaycastHit hit) { }
#if UNITY_EDITOR
		[Button]
		void TestDamage(float damage = 100)
		{
			var DEBUGDAMAGEPROVIDER = gameObject.AddComponent<MCDamageProvider>();
			// DEBUGDAMAGEPROVIDER.damage = damage;
			// HandleDamageProvider(DEBUGDAMAGEPROVIDER,damage,head.position,Vector3.back);
			Destroy(DEBUGDAMAGEPROVIDER);
		}

		[Button]
		void GatherChildrenBlinkingRenderers()
		{
			blinkRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>());
		}
	#endif
    }
}
