using AssetLookupTree;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.DuelScene.VFX;

namespace MovementSystem;

public class SplineEventPayloadVFXALT : SplineEventTrigger
{
	private readonly IVfxProvider _vfxProvider;

	private readonly VfxData _vfxData;

	private readonly ICardDataAdapter _relevantCard;

	private readonly Transform _overrideLocal;

	public SplineEventPayloadVFXALT(VfxData vfxData, ICardDataAdapter relevantCard, Transform overrideLocal, IVfxProvider vfxProvider)
		: base(vfxData.PrefabData.StartTime)
	{
		_vfxProvider = vfxProvider;
		_vfxData = vfxData;
		_relevantCard = relevantCard;
		_overrideLocal = overrideLocal;
	}

	protected override bool CanUpdate()
	{
		if (_vfxProvider != null)
		{
			return _vfxData != null;
		}
		return false;
	}

	protected override void Trigger(float progress)
	{
		_vfxProvider.PlayVFX(_vfxData, _relevantCard, null, _overrideLocal);
	}
}
