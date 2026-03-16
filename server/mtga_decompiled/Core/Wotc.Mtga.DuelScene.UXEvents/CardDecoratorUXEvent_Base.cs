using System.Collections.Generic;
using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.DuelScene.UXEvents;

public abstract class CardDecoratorUXEvent_Base : UXEvent
{
	protected HashSet<DecoratorType> _decorators;

	protected VfxData _vfxData;

	protected SfxData _sfxData;

	protected readonly GameManager _gameManager;

	protected readonly AssetLookupSystem _assetLookupSystem;

	protected readonly IVfxProvider _vfxProvider;

	private readonly CardHolderReference<StackCardHolder> _stack;

	protected PropertyType _associatedPropertyType;

	public MtgCardInstance AffectorCard { get; private set; }

	public MtgCardInstance AffectedCard { get; private set; }

	protected CardDecoratorUXEvent_Base(MtgCardInstance affector, MtgCardInstance affected, HashSet<DecoratorType> decorators, PropertyType associatedPropertyType, GameManager gameManager)
	{
		AffectorCard = affector;
		AffectedCard = affected;
		_decorators = decorators;
		_gameManager = gameManager;
		_assetLookupSystem = gameManager.AssetLookupSystem;
		_vfxProvider = gameManager.VfxProvider;
		_associatedPropertyType = associatedPropertyType;
		_stack = CardHolderReference<StackCardHolder>.Stack(gameManager.CardHolderManager);
		if (AffectorCard == null || AffectedCard == null || _decorators == null || _decorators.Count == 0)
		{
			Complete();
		}
		else
		{
			GetPayloadData();
		}
	}

	protected abstract void GetPayloadData();

	public override void Execute()
	{
		if (_vfxData != null && _gameManager.ViewManager.TryGetCardView(AffectorCard.InstanceId, out var cardView))
		{
			_vfxProvider.PlayVFX(_vfxData, cardView.Model, AffectedCard);
		}
		if (_sfxData != null)
		{
			AudioManager.PlayAudio(_sfxData.AudioEvents, _stack.Get().gameObject);
		}
		Complete();
	}

	protected override void Cleanup()
	{
		_stack.ClearCache();
		base.Cleanup();
	}
}
