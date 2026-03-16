using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using ReferenceMap;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class AppendDamagesEntityText : ITextEntryParser
{
	private const uint ABILITY_ID_REPERCUSSION = 98425u;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	private readonly IAbilityDataProvider _abilityDataProvider;

	private readonly IClientLocProvider _locProvider;

	private readonly Func<MtgGameState> _getCurrentGameState;

	public AppendDamagesEntityText(IEntityNameProvider<uint> entityNameProvider, IAbilityDataProvider abilityDataProvider, IClientLocProvider locProvider, Func<MtgGameState> getCurrentGameState)
	{
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
		_abilityDataProvider = abilityDataProvider ?? NullAbilityDataProvider.Default;
		_locProvider = locProvider ?? NullLocProvider.Default;
		_getCurrentGameState = getCurrentGameState ?? ((Func<MtgGameState>)(() => new MtgGameState()));
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (CanAddEntry(card))
		{
			string name = _entityNameProvider.GetName(TriggeredByControllerId(card.InstanceId));
			if (!string.IsNullOrEmpty(name))
			{
				yield return new BasicTextEntry(_locProvider.GetLocalizedTextForLanguage("DuelScene/RuleText/DamagesEntity", overrideLang, ("entityName", name)));
			}
		}
	}

	private uint TriggeredByControllerId(uint cardId)
	{
		MtgGameState mtgGameState = _getCurrentGameState();
		if (mtgGameState == null)
		{
			return 0u;
		}
		uint triggeredById = mtgGameState.ReferenceMap.GetTriggeredById(cardId);
		if (!mtgGameState.TryGetCard(triggeredById, out var card))
		{
			return 0u;
		}
		return card.Controller?.InstanceId ?? 0;
	}

	private bool CanAddEntry(ICardDataAdapter card)
	{
		if (card.ObjectType == GameObjectType.Ability && _abilityDataProvider.TryGetAbilityPrintingById(card.GrpId, out var ability))
		{
			return ability.Id == 98425;
		}
		return false;
	}
}
