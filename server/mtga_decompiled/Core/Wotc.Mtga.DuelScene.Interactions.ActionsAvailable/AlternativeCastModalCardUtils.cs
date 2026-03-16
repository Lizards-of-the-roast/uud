using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga.Logging;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class AlternativeCastModalCardUtils
{
	public static (CardData, AbilityPrintingData) GetAlternativeCastModalData(Wotc.Mtgo.Gre.External.Messaging.Action action, CardData originalCardData, MtgCardInstance instanceCopy, CardPrintingData printingCopy, IReadOnlyList<ManaQuantity> interactionCost, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		CardData cardData;
		AbilityPrintingData abilityPrintingData;
		if (originalCardData.InstanceId == action.SourceId && action.AbilityGrpId != 115)
		{
			(cardData, abilityPrintingData) = CreateAbilityOverrideModalData(action, originalCardData, instanceCopy, printingCopy, interactionCost, cardDatabase);
		}
		else
		{
			abilityPrintingData = GetAssociatedModalCastAbility(cardDatabase.AbilityDataProvider, action, gameState);
			if (action.ActionType != ActionType.Cast)
			{
				cardData = ((!action.IsCastAction() && !action.IsPlayAction()) ? CardDataExtensions.CreateAbilityCard(abilityPrintingData, originalCardData, cardDatabase) : CreateAltCastModal(action, originalCardData, instanceCopy, printingCopy, interactionCost, cardDatabase));
			}
			else
			{
				bool num = originalCardData.Instance.Abilities.ContainsId(action.AlternativeGrpId);
				MtgCardInstance mtgCardInstance = (num ? originalCardData.Instance.GetCopy() : instanceCopy.GetCopy());
				CardPrintingData cardPrintingData = (num ? originalCardData.Printing : printingCopy);
				if (!cardPrintingData.TextChangeData.Equals(null) && (abilityPrintingData.Id == cardPrintingData.TextChangeData.ChangeSourceId || abilityPrintingData.BaseId == cardPrintingData.TextChangeData.ChangeSourceId))
				{
					int num2 = mtgCardInstance.Abilities.FindIndex(cardPrintingData.TextChangeData, (AbilityPrintingData ability, TextChange textChange) => ability.Id == textChange.OriginalAbilityId);
					AbilityPrintingData abilityPrintingById = cardDatabase.AbilityDataProvider.GetAbilityPrintingById(cardPrintingData.TextChangeData.ChangedAbilityId);
					if (num2 >= 0 && abilityPrintingById != null)
					{
						mtgCardInstance.Abilities.RemoveAt(num2);
						mtgCardInstance.Abilities.Insert(num2, abilityPrintingById);
					}
				}
				cardData = ((!ModalCardUtils.IsFaceDownCastId(action.AlternativeGrpId)) ? CreateAltCastModal(action, originalCardData, mtgCardInstance, cardPrintingData, interactionCost, cardDatabase) : ModalCardUtils.CreateFaceDownCastingModal(originalCardData, new List<ManaRequirement>(action.ManaCost), cardDatabase, action.AlternativeGrpId));
			}
		}
		cardData.Instance.ParentId = originalCardData.InstanceId;
		cardData.Instance.Parent = originalCardData.Instance.GetCopy();
		cardData.Instance.SkinCode = originalCardData.SkinCode;
		cardData.Instance.SleeveCode = originalCardData.SleeveCode;
		return (cardData, abilityPrintingData);
	}

	public static AbilityPrintingData GetAssociatedModalCastAbility(IAbilityDataProvider abilityProvider, Wotc.Mtgo.Gre.External.Messaging.Action action, MtgGameState gameState)
	{
		if (abilityProvider.TryGetAbilityPrintingById(action.AlternativeGrpId, out var ability))
		{
			return ability;
		}
		if (action.TryGetSourceInstance(gameState, out var sourceInstance))
		{
			if (sourceInstance.Abilities.TryGetById(action.AlternativeGrpId, out var printing))
			{
				return printing;
			}
			if (sourceInstance.Abilities.TryGetByReferenceType((AbilityType)action.AlternativeGrpId, out var printing2))
			{
				return printing2;
			}
		}
		return AbilityPrintingData.InvalidAbility(action.AlternativeGrpId);
	}

	private static (CardData, AbilityPrintingData) CreateAbilityOverrideModalData(Wotc.Mtgo.Gre.External.Messaging.Action action, CardData originalCardData, MtgCardInstance instanceCopy, CardPrintingData printingCopy, IEnumerable<ManaQuantity> interactionCost, ICardDatabaseAdapter cardDatabase)
	{
		new UnityLogger("Unexpected code run in client", LoggerLevel.Error).LogError($"CreateAbilityOverrideModalData : CardGrpId = '{originalCardData.GrpId}', ActionGrpId = '{action.GrpId}', AbilityGrpId = '{action.AbilityGrpId}'");
		CardData cardData;
		AbilityPrintingData abilityPrintingData;
		if (action.ActionType == ActionType.Cast)
		{
			MtgCardInstance copy = instanceCopy.GetCopy();
			CardPrintingRecord record = printingCopy.Record;
			IReadOnlyList<(uint, uint)> abilityIds = Array.Empty<(uint, uint)>();
			string oldSchoolManaText = ((interactionCost != null) ? ManaUtilities.ConvertToOldSchoolManaText(interactionCost) : null);
			cardData = new CardData(copy, new CardPrintingData(printingCopy, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, oldSchoolManaText, null, null, null, null, null, null, null, null, null, null, null, null, null, abilityIds)));
			cardData.Instance.Abilities.Clear();
			abilityPrintingData = cardDatabase.AbilityDataProvider.GetAbilityPrintingById(action.AlternativeGrpId);
			if (abilityPrintingData == null)
			{
				abilityPrintingData = originalCardData.Abilities.GetById(action.AlternativeGrpId);
			}
		}
		else
		{
			abilityPrintingData = cardDatabase.AbilityDataProvider.GetAbilityPrintingById(action.AlternativeGrpId);
			if (abilityPrintingData == null)
			{
				abilityPrintingData = originalCardData.Abilities.GetById(action.AlternativeGrpId);
			}
			cardData = CardDataExtensions.CreateAbilityCard(abilityPrintingData, originalCardData, cardDatabase);
		}
		cardData.Instance.SkinCode = originalCardData.SkinCode;
		cardData.Instance.SleeveCode = originalCardData.SleeveCode;
		cardData.RulesTextOverride = new AbilityTextOverride(cardDatabase, originalCardData.TitleId).AddAbility(abilityPrintingData.Id).AddSource(originalCardData.Instance).AddSource(originalCardData.Printing);
		return (cardData, abilityPrintingData);
	}

	private static CardData CreateAltCastModal(Wotc.Mtgo.Gre.External.Messaging.Action action, CardData originalCard, MtgCardInstance instanceCopy, CardPrintingData printingCopy, IReadOnlyCollection<ManaQuantity> interactionCost, ICardDatabaseAdapter cardDatabase)
	{
		switch (action.ActionType)
		{
		default:
		{
			CardPrintingData printing;
			if (interactionCost != null)
			{
				CardPrintingRecord record = printingCopy.Record;
				string oldSchoolManaText = ManaUtilities.ConvertToOldSchoolManaText(interactionCost);
				printing = new CardPrintingData(printingCopy, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, oldSchoolManaText));
			}
			else
			{
				printing = printingCopy;
			}
			return ModalCardUtils.CreateModalCard(action, instanceCopy, printing, interactionCost);
		}
		case ActionType.CastLeft:
		case ActionType.CastRight:
			return ModalCardUtils.CreateModalSplitCard(action, originalCard, interactionCost, cardDatabase.CardDataProvider);
		case ActionType.CastAdventure:
			return ModalCardUtils.CreateOmenOrAdventureSubCard(action, originalCard, interactionCost, cardDatabase.CardDataProvider, GameObjectType.Adventure);
		case ActionType.CastOmen:
			return ModalCardUtils.CreateOmenOrAdventureSubCard(action, originalCard, interactionCost, cardDatabase.CardDataProvider, GameObjectType.Omen);
		case ActionType.CastLeftRoom:
		case ActionType.CastRightRoom:
			return ModalCardUtils.CreateRoomSubCard(action, originalCard, cardDatabase, interactionCost);
		case ActionType.CastMdfc:
		case ActionType.PlayMdfc:
		case ActionType.CastPrototype:
			return ModalCardUtils.CreateModalLinkedFace(action, originalCard, interactionCost, cardDatabase);
		}
	}
}
