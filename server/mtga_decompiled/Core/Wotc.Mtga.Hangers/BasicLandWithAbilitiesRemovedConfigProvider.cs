using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class BasicLandWithAbilitiesRemovedConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locManager;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public BasicLandWithAbilitiesRemovedConfigProvider(IClientLocProvider locManager, IGameStateProvider gameStateProvider, ICardDatabaseAdapter cardDatabase)
	{
		_locManager = locManager;
		_gameStateProvider = gameStateProvider;
		_cardDatabase = cardDatabase;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (!IsBasicLandMissingIntrinsicAbility(model) || model.Instance == null)
		{
			yield break;
		}
		string header = ((model.Instance.AbilityRemovers.Count != 0) ? _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Abilities_Removed_Title") : _locManager.GetLocalizedText("AbilityHanger/PlayWarning/Ability_Removed_Title"));
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		List<string> list = new List<string>();
		foreach (LayeredEffectData layeredEffect in model.Instance.LayeredEffects)
		{
			if (layeredEffect.AnnotationTypes.Contains(AnnotationType.RemoveAbility) && mtgGameState.TryGetCard(layeredEffect.AffectorId, out var card))
			{
				string localizedText = _cardDatabase.GreLocProvider.GetLocalizedText(card.TitleId);
				list.Add(localizedText);
			}
		}
		string details = string.Join(", ", list);
		yield return new HangerConfig(header, details);
	}

	public static bool IsBasicLandMissingIntrinsicAbility(ICardDataAdapter model)
	{
		if (model.Supertypes.Contains(SuperType.Basic) && model.CardTypes.Contains(CardType.Land))
		{
			if ((!model.Subtypes.Contains(SubType.Plains) || model.AbilityIds.Contains(1001u)) && (!model.Subtypes.Contains(SubType.Island) || model.AbilityIds.Contains(1002u)) && (!model.Subtypes.Contains(SubType.Swamp) || model.AbilityIds.Contains(1003u)) && (!model.Subtypes.Contains(SubType.Mountain) || model.AbilityIds.Contains(1004u)))
			{
				if (model.Subtypes.Contains(SubType.Forest))
				{
					return !model.AbilityIds.Contains(1005u);
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
