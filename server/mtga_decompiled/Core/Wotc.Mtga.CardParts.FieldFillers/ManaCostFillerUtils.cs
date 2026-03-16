using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.ManaCostOverride;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.ActionCalculators;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.CardParts.FieldFillers;

public static class ManaCostFillerUtils
{
	public enum ManaCostOverride
	{
		None,
		PrintedManaCost,
		ManaCostOverride
	}

	public enum ModifiedComparer
	{
		PrintedManaCost,
		ForetellCost,
		AbilityCost
	}

	private static readonly IComparer<IEnumerable<ManaQuantity>> _castCostComparer = new CastCostComparer();

	private static readonly IActionPriorityCalculator ActionPriorityCalculator = new ActionPriorityCalculator();

	private static List<ManaQuantity> _sorter = new List<ManaQuantity>();

	public static string GetText(CDCManaCostFiller.FieldType fieldType, ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, WorkflowBase currentInteraction, ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		switch (fieldType)
		{
		case CDCManaCostFiller.FieldType.ManaCost:
			if (model.IsSplitCard(ignoreInstances: true) || model.IsRoomParent())
			{
				return SplitCardCost(model, cardHolderType, gameState, currentInteraction, cardDatabase, assetLookupSystem, colorSettings, out highlightMana);
			}
			if (cardHolderType == CardHolderType.Hand)
			{
				if (model.IsPrototypeParent())
				{
					return PrototypeCost(model, cardHolderType, gameState, currentInteraction, cardDatabase, assetLookupSystem, colorSettings, out highlightMana);
				}
				if (model.IsOmenOrAdventureCard(ignoreInstances: true))
				{
					return OmenOrAdventureCost(model, cardHolderType, gameState, currentInteraction, cardDatabase, assetLookupSystem, colorSettings, out highlightMana);
				}
				if (model.IsMDFCCard())
				{
					return MDFCCost(model, cardHolderType, gameState, currentInteraction, cardDatabase, assetLookupSystem, colorSettings, out highlightMana);
				}
			}
			return GetManaCostText(GetLowestCostAction(model, gameState, currentInteraction, cardDatabase), model, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		case CDCManaCostFiller.FieldType.SimpleManaCost:
			return GetManaCostText(GetLowestCostAction(model, gameState, currentInteraction, cardDatabase), model, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		default:
			highlightMana = false;
			return string.Empty;
		}
	}

	private static string SplitManaText(string manaOne, string manaTwo)
	{
		return manaOne + "<#FFFFFF>/</color>" + manaTwo;
	}

	private static string SplitCardCost(ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, WorkflowBase currentInteraction, ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, cardDatabase.CardDataProvider);
		string manaCostText = GetManaCostText(GetLowestCostAction(linkedFaceAtIndex, gameState, currentInteraction, cardDatabase), linkedFaceAtIndex, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		ICardDataAdapter linkedFaceAtIndex2 = model.GetLinkedFaceAtIndex(1, ignoreInstance: false, cardDatabase.CardDataProvider);
		string manaCostText2 = GetManaCostText(GetLowestCostAction(linkedFaceAtIndex2, gameState, currentInteraction, cardDatabase), linkedFaceAtIndex2, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		return $"{manaCostText}<#FFFFFF>/</color>{manaCostText2}";
	}

	private static string PrototypeCost(ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, WorkflowBase currentInteraction, ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, cardDatabase.CardDataProvider);
		Wotc.Mtgo.Gre.External.Messaging.Action lowestCostAction = GetLowestCostAction(linkedFaceAtIndex, gameState, currentInteraction, cardDatabase);
		string manaCostText = GetManaCostText(lowestCostAction, linkedFaceAtIndex, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		string manaCostText2 = GetManaCostText(GetLowestCostAction(model, gameState, currentInteraction, cardDatabase), model, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		if (lowestCostAction == null || lowestCostAction.SourceId == linkedFaceAtIndex.InstanceId)
		{
			return manaCostText2;
		}
		return SplitManaText(manaCostText, manaCostText2);
	}

	private static string OmenOrAdventureCost(ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, WorkflowBase currentInteraction, ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, cardDatabase.CardDataProvider);
		Wotc.Mtgo.Gre.External.Messaging.Action lowestCostAction = GetLowestCostAction(linkedFaceAtIndex, gameState, currentInteraction, cardDatabase);
		string manaCostText = GetManaCostText(lowestCostAction, linkedFaceAtIndex, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		Wotc.Mtgo.Gre.External.Messaging.Action lowestCostAction2 = GetLowestCostAction(model, gameState, currentInteraction, cardDatabase);
		string manaCostText2 = GetManaCostText(lowestCostAction2, model, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		if (lowestCostAction == null || lowestCostAction.SourceId == linkedFaceAtIndex.InstanceId)
		{
			return manaCostText2;
		}
		if (lowestCostAction2 == null)
		{
			return manaCostText;
		}
		return SplitManaText(manaCostText, manaCostText2);
	}

	private static string MDFCCost(ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, WorkflowBase currentInteraction, ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, cardDatabase.CardDataProvider);
		Wotc.Mtgo.Gre.External.Messaging.Action lowestCostAction = GetLowestCostAction(linkedFaceAtIndex, gameState, currentInteraction, cardDatabase);
		string manaCostText = GetManaCostText(lowestCostAction, linkedFaceAtIndex, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		string manaCostText2 = GetManaCostText(GetLowestCostAction(model, gameState, currentInteraction, cardDatabase), model, cardHolderType, gameState, cardDatabase.AbilityDataProvider, assetLookupSystem, colorSettings, out highlightMana);
		if (CardUtilities.IsLand(linkedFaceAtIndex) || lowestCostAction == null || lowestCostAction.ManaCost.Count == 0)
		{
			return manaCostText2;
		}
		return SplitManaText(manaCostText2, manaCostText);
	}

	private static string GetManaCostText(Wotc.Mtgo.Gre.External.Messaging.Action costAction, ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, IAbilityDataProvider abilityProvider, AssetLookupSystem assetLookupSystem, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		if (HasManaCostOverride(model, costAction, cardHolderType, assetLookupSystem, out var payload))
		{
			return GetOverrideText(model, costAction, payload, abilityProvider, colorSettings, out highlightMana);
		}
		if (CanUseCastCost(costAction, gameState, model, cardHolderType) || CanUseActivateCost(costAction, gameState, cardHolderType) || costAction.IsActionType(ActionType.Special))
		{
			return GetActionCostText(costAction, model, out highlightMana);
		}
		if (model.ObjectType == GameObjectType.Token && model.ConvertedManaCost == 0)
		{
			highlightMana = false;
			return string.Empty;
		}
		if (model.Instance != null && model.Instance.IsCopy && !string.IsNullOrEmpty(model.Instance.CopyExceptionManaString))
		{
			highlightMana = false;
			return ManaUtilities.StripCurlyBracesFromManaSymbols(model.Instance.CopyExceptionManaString);
		}
		if (PerpetualChangeUtilities.TryGetPerpetualCost(model, out var result))
		{
			highlightMana = model.OldSchoolManaText != result;
			return result;
		}
		highlightMana = false;
		return model.OldSchoolManaText;
	}

	private static string GetActionCostText(Wotc.Mtgo.Gre.External.Messaging.Action costAction, ICardDataAdapter model, out bool highlightMana)
	{
		ActionType actionType = costAction?.ActionType ?? ActionType.None;
		IReadOnlyList<ManaQuantity> readOnlyList = ((costAction != null) ? costAction.ConvertedActionManaCost(model) : model.PrintedCastingCost);
		if (((readOnlyList.Count == 1 && readOnlyList[0].Count == 0) || readOnlyList.Count == 0) && actionType == ActionType.Activate && !ActionExtensions.IsEmbalmAbilityAction(model, costAction))
		{
			highlightMana = false;
			return string.Empty;
		}
		highlightMana = ManaQuantitiesAreNotEqual(readOnlyList, model.PrintedCastingCost);
		return SortedManaCostText(readOnlyList);
	}

	private static string GetOverrideText(ICardDataAdapter model, Wotc.Mtgo.Gre.External.Messaging.Action costAction, ManaCostOverridePayload payload, IAbilityDataProvider abilityProvider, CardTextColorSettings colorSettings, out bool highlightMana)
	{
		IReadOnlyCollection<ManaQuantity> readOnlyCollection = payload.ManaCostOverride switch
		{
			ManaCostOverride.PrintedManaCost => model.PrintedCastingCost, 
			ManaCostOverride.ManaCostOverride => model.ManaCostOverride, 
			_ => costAction?.ConvertedActionManaCost() ?? model.PrintedCastingCost, 
		};
		highlightMana = ManaQuantitiesAreNotEqual(payload.ModifiedComparer switch
		{
			ModifiedComparer.ForetellCost => ManaUtilities.ForetellManaCost, 
			ModifiedComparer.AbilityCost => (IEnumerable<ManaQuantity>)(((object)abilityProvider.GetAbilityPrintingById(model.GrpId)?.ManaCost) ?? ((object)Array.Empty<ManaQuantity>())), 
			_ => model.PrintedCastingCost, 
		}, readOnlyCollection);
		string text = SortedManaCostText(readOnlyCollection);
		if (!string.IsNullOrEmpty(payload.CostFormat))
		{
			text = string.Format(colorSettings.DefaultFormat, string.Format(payload.CostFormat, text));
		}
		return text;
	}

	private static string SortedManaCostText(IReadOnlyCollection<ManaQuantity> manaCost)
	{
		_sorter.Clear();
		_sorter.AddRange(manaCost);
		_sorter.Sort(ManaQuantity.SortComparison);
		return ManaUtilities.ConvertToOldSchoolManaText(_sorter);
	}

	private static Wotc.Mtgo.Gre.External.Messaging.Action GetLowestCostAction(ICardDataAdapter model, MtgGameState gameState, WorkflowBase currentInteraction, ICardDatabaseAdapter cardDatabase)
	{
		if (model.Instance == null)
		{
			return null;
		}
		uint num = ((model.Instance.ParentId != 0) ? model.Instance.ParentId : model.InstanceId);
		IReadOnlyCollection<GreInteraction> interactions;
		IReadOnlyCollection<ActionInfo> gsActions;
		if (model.LinkedFaceType == LinkedFace.SplitCard && model.LinkedFaceInstances.Count == 1)
		{
			ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, cardDatabase.CardDataProvider);
			interactions = ((currentInteraction != null) ? ActionsAvailableWorkflow.GetInteractionsForId(linkedFaceAtIndex.InstanceId, currentInteraction) : null);
			gsActions = linkedFaceAtIndex.Actions;
		}
		else
		{
			interactions = ActionsAvailableWorkflow.GetInteractionsForId(num, currentInteraction);
			IReadOnlyList<ActionInfo> readOnlyList = gameState?.GetActionsForCardId(num);
			gsActions = readOnlyList ?? model.Actions;
		}
		return ActionPriorityCalculator.GetPrioritizedAction(cardDatabase.AbilityDataProvider, interactions, gsActions, ManaUtilities.GetActionTypeFilter(model.ObjectType));
	}

	private static bool HasManaCostOverride(ICardDataAdapter model, Wotc.Mtgo.Gre.External.Messaging.Action costAction, CardHolderType cardHolderType, AssetLookupSystem assetLookupSystem, out ManaCostOverridePayload payload)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.GreAction = costAction;
		assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ManaCostOverridePayload> loadedTree))
		{
			ManaCostOverridePayload payload2 = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				payload = payload2;
				return true;
			}
		}
		payload = null;
		return false;
	}

	public static bool CanUseCastCost(Wotc.Mtgo.Gre.External.Messaging.Action action, MtgGameState gameState, ICardDataAdapter model, CardHolderType cardHolderType)
	{
		if (!action.IsCastAction())
		{
			return false;
		}
		if (gameState == null)
		{
			return false;
		}
		if (model.Instance == null)
		{
			return false;
		}
		if (model.ObjectType == GameObjectType.RevealedCard)
		{
			return false;
		}
		return CastAdjacentZone(cardHolderType, model.ZoneType, model.IsDisplayedFaceDown);
	}

	private static bool CastAdjacentZone(CardHolderType cardHolderType, ZoneType zoneType, bool isFacedown)
	{
		switch (cardHolderType)
		{
		case CardHolderType.Examine:
			return false;
		default:
			if (zoneType != ZoneType.Hand && zoneType != ZoneType.Stack)
			{
				if (cardHolderType == CardHolderType.Library)
				{
					return !isFacedown;
				}
				return false;
			}
			break;
		case CardHolderType.Hand:
		case CardHolderType.Stack:
			break;
		}
		return true;
	}

	public static bool CanUseActivateCost(Wotc.Mtgo.Gre.External.Messaging.Action action, MtgGameState gameState, CardHolderType cardHolderType)
	{
		if (!action.IsActionType(ActionType.Activate))
		{
			return false;
		}
		if (gameState == null)
		{
			return false;
		}
		MtgCardInstance cardById = gameState.GetCardById(action.InstanceId);
		if (cardById == null)
		{
			return false;
		}
		if (cardById.ObjectType == GameObjectType.RevealedCard)
		{
			return false;
		}
		MtgZone zone = cardById.Zone;
		if (zone == null)
		{
			return false;
		}
		bool num = zone.Type == ZoneType.Graveyard || zone.Type == ZoneType.Exile;
		bool flag = cardHolderType == CardHolderType.Hand || cardHolderType == CardHolderType.Examine;
		return num && flag;
	}

	public static bool ManaQuantitiesAreNotEqual(IEnumerable<ManaQuantity> lhs, IEnumerable<ManaQuantity> rhs)
	{
		return _castCostComparer.Compare(lhs, rhs) != 0;
	}
}
