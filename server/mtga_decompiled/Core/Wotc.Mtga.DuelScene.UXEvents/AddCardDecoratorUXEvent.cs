using System.Collections.Generic;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class AddCardDecoratorUXEvent : CardDecoratorUXEvent_Base
{
	private const string TOSTRING_NULL_AFFECTOR = "[NULL AFFECTOR]";

	private const string TOSTRING_NULL_AFFECTED = "[NULL AFFECTED]";

	private const string TOSTRING_HEADER_FORMAT = "Add Decorator Event: {0} affects {1}";

	private const string TOSTRING_ADDED_DECORATORS = "Added Decorators";

	private const string TOSTRING_NO_DECORATORS = "No Added Decorators!";

	private const string TOSTRING_TAB = "     ";

	public AddCardDecoratorUXEvent(MtgCardInstance affector, MtgCardInstance affected, HashSet<DecoratorType> decorators, PropertyType associatedPropertyType, GameManager gameManager)
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
		_assetLookupSystem.Blackboard.UpdatedProperties = new HashSet<PropertyType> { _associatedPropertyType };
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DecoratorGainSFX> loadedTree))
		{
			DecoratorGainSFX payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_sfxData = payload.SfxData;
			}
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DecoratorGainVFX> loadedTree2))
		{
			DecoratorGainVFX payload2 = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				_vfxData = payload2.VfxData;
			}
		}
	}

	public override string ToString()
	{
		string arg = ((base.AffectorCard != null) ? base.AffectedCard.InstanceId.ToString() : "[NULL AFFECTOR]");
		string arg2 = ((base.AffectedCard != null) ? base.AffectedCard.InstanceId.ToString() : "[NULL AFFECTED]");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"Add Decorator Event: {arg} affects {arg2}");
		stringBuilder.AppendLine();
		stringBuilder.Append("     ");
		if (_decorators.Count > 0)
		{
			stringBuilder.Append("Added Decorators");
			foreach (DecoratorType decorator in _decorators)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("     ");
				stringBuilder.Append("     ");
				stringBuilder.Append(decorator.ToString());
			}
		}
		else
		{
			stringBuilder.Append("No Added Decorators!");
		}
		return stringBuilder.ToString();
	}
}
