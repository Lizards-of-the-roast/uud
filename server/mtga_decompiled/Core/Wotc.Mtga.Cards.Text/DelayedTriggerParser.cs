using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class DelayedTriggerParser : ITextEntryParser
{
	private const uint KICKER_BASE_ID = 34u;

	private readonly IClientLocProvider _locManager;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly Func<MtgGameState> _getCurrentGameState;

	public DelayedTriggerParser(IClientLocProvider locManager, AssetLookupSystem assetLookupSystem, Func<MtgGameState> getCurrentGameState)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_getCurrentGameState = getCurrentGameState ?? ((Func<MtgGameState>)(() => (MtgGameState)null));
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (CardIsKickerDelayedTrigger(card))
		{
			string key = (ParentWasKicked(card) ? "Card/Textbox/Kicked" : "Card/Textbox/NotKicked");
			yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, _locManager.GetLocalizedTextForLanguage(key, overrideLang)));
		}
		if (_getCurrentGameState() == null || !card.AbilityIds.Intersect(_getCurrentGameState().DelayedTriggerAffectees.Select((DelayedTriggerData d) => d.AbilityId)).Any() || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AbilityWordSupplementalTextPayload> supplementalTextTree))
		{
			yield break;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(card);
		_assetLookupSystem.Blackboard.GameState = _getCurrentGameState();
		foreach (AbilityPrintingData ability in card.Abilities)
		{
			_assetLookupSystem.Blackboard.Ability = ability;
			AbilityWordSupplementalTextPayload payload = supplementalTextTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				string localizedTextForLanguage = _locManager.GetLocalizedTextForLanguage(payload.LocKey, overrideLang, payload.BuildParameters(_assetLookupSystem.Blackboard));
				yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, localizedTextForLanguage));
			}
		}
	}

	private static bool CardIsKickerDelayedTrigger(ICardDataAdapter card)
	{
		if (card == null || card.Parent == null)
		{
			return false;
		}
		if (card.ObjectType != GameObjectType.TriggerHolder)
		{
			return false;
		}
		if (card.Parent.Abilities.Exists((AbilityPrintingData x) => x.BaseId == 34))
		{
			return card.Parent.Abilities.Exists((AbilityPrintingData x) => card.GrpId == x.Id);
		}
		return false;
	}

	private static bool ParentWasKicked(ICardDataAdapter card)
	{
		if (card == null || card.Parent == null)
		{
			return false;
		}
		return card.Parent.CastingTimeOptions.Any((CastingTimeOption x) => x.IsKicker);
	}
}
