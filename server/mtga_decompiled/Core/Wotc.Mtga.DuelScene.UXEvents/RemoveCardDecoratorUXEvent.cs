using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class RemoveCardDecoratorUXEvent : CardDecoratorUXEvent_Base
{
	public RemoveCardDecoratorUXEvent(MtgCardInstance affector, MtgCardInstance affected, HashSet<DecoratorType> decorators, PropertyType associatedPropertyType, GameManager gameManager)
		: base(affector, affected, decorators, associatedPropertyType, gameManager)
	{
	}

	protected override void GetPayloadData()
	{
		CardData cardDataExtensive = CardDataExtensions.CreateWithDatabase(base.AffectorCard, _gameManager.CardDatabase);
		CardHolderType cardHolderType = ((base.AffectorCard.Zone != null) ? base.AffectorCard.Zone.Type.ToCardHolderType() : CardHolderType.None);
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.SetCardDataExtensive(cardDataExtensive);
		_assetLookupSystem.Blackboard.DecoratorTypes = _decorators;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DecoratorLossSFX> loadedTree))
		{
			DecoratorLossSFX payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_sfxData = payload.SfxData;
			}
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DecoratorLossVFX> loadedTree2))
		{
			DecoratorLossVFX payload2 = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				_vfxData = payload2.VfxData;
			}
		}
	}
}
