using Core.Meta.MainNavigation.BoosterChamber;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BoosterMetaCardView : CDCMetaCardView
{
	[SerializeField]
	private Transform _anticipationParticlesParent;

	[SerializeField]
	[FormerlySerializedAs("_anticipationParticlesPrefab")]
	private ParticleSystem _rareAnticipationParticlesPrefab;

	[SerializeField]
	private ParticleSystem _mythicAnticipationParticlesPrefab;

	[SerializeField]
	private Transform _revealParticlesParent;

	[SerializeField]
	private ParticleSystem _rareRevealParticlesPrefab;

	[SerializeField]
	private ParticleSystem _mythicRareRevealParticlesPrefab;

	private ParticleSystem _anticipationParticlesInstance;

	private ParticleSystem _revealParticlesInstance;

	private BoosterCardHolder _holder;

	protected override bool ShowHighlight => false;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (_holder == null)
		{
			_holder = GetComponentInParent<BoosterCardHolder>();
		}
		if (_holder == null || (_holder != null && _holder.InteractionsAllowed))
		{
			base.OnPointerEnter(eventData);
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (_holder == null || (_holder != null && _holder.InteractionsAllowed))
		{
			OnPointerExit(eventData);
			base.OnBeginDrag(eventData);
		}
	}

	public void StartRevealParticles()
	{
		ParticleSystem particleSystem = Object.Instantiate((base.Card.Rarity == CardRarity.MythicRare) ? _mythicRareRevealParticlesPrefab : _rareRevealParticlesPrefab, _revealParticlesParent, worldPositionStays: true);
		particleSystem.transform.localPosition = Vector3.zero;
		particleSystem.transform.localRotation = Quaternion.Euler(Vector3.zero);
		particleSystem.transform.localScale = Vector3.one;
		_revealParticlesInstance = particleSystem;
	}

	public void StartAnticipationParticles(bool offsetStart = true)
	{
		ParticleSystem particleSystem = Object.Instantiate((base.Card.Rarity == CardRarity.MythicRare && _mythicAnticipationParticlesPrefab != null) ? _mythicAnticipationParticlesPrefab : _rareAnticipationParticlesPrefab, _anticipationParticlesParent, worldPositionStays: true);
		particleSystem.transform.localPosition = Vector3.zero;
		particleSystem.transform.localRotation = Quaternion.Euler(Vector3.zero);
		particleSystem.transform.localScale = Vector3.one;
		particleSystem.Simulate(offsetStart ? Random.Range(0f, 10f) : 0f);
		particleSystem.Play();
		_anticipationParticlesInstance = particleSystem;
	}

	public void StopAnticipationParticles()
	{
		if (_anticipationParticlesInstance != null)
		{
			ParticleSystem anticipationParticlesInstance = _anticipationParticlesInstance;
			_anticipationParticlesInstance = null;
			anticipationParticlesInstance.Stop(withChildren: true);
			Object.Destroy(anticipationParticlesInstance.gameObject);
		}
	}

	public void RemoveParticles()
	{
		if (_anticipationParticlesInstance != null)
		{
			ParticleSystem anticipationParticlesInstance = _anticipationParticlesInstance;
			_anticipationParticlesInstance = null;
			anticipationParticlesInstance.Stop(withChildren: true);
			Object.Destroy(anticipationParticlesInstance.gameObject);
		}
		if (_revealParticlesInstance != null)
		{
			ParticleSystem revealParticlesInstance = _revealParticlesInstance;
			_revealParticlesInstance = null;
			revealParticlesInstance.Stop(withChildren: true);
			Object.Destroy(revealParticlesInstance.gameObject);
		}
	}

	public override void UpdateNumberNew(uint grpId)
	{
	}

	private void OnDisable()
	{
		OnPointerExit(null);
		_holder = null;
	}

	public void EnableCollider(bool enabled)
	{
		_cardCollider.enabled = enabled;
	}
}
