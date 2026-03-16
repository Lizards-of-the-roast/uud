using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.ActionCalculators;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ModifiedCastingCostProvider : IHangerConfigProvider
{
	private const string LOC_PARAM_NEWCOST = "newCost";

	private static readonly IActionPriorityCalculator _actionPriorityCalculator = new ActionPriorityCalculator();

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _locProvider;

	private readonly IActionProvider _actionProvider;

	private readonly IActionHangerConfigProvider _actionOverrideProvider;

	public ModifiedCastingCostProvider(ICardDatabaseAdapter cardDatabase, IClientLocProvider locProvider, IActionProvider actionProvider, IActionHangerConfigProvider actionOverrideProvider)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_locProvider = locProvider ?? NullLocProvider.Default;
		_actionProvider = actionProvider ?? NullActionProvider.Default;
		_actionOverrideProvider = actionOverrideProvider ?? NullActionHangerConfigProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (!IsValidObjectType(model.ObjectType))
		{
			yield break;
		}
		string text = string.Empty;
		uint instanceId = model.InstanceId;
		IReadOnlyList<GreInteraction> requestActions = _actionProvider.GetRequestActions(instanceId);
		IReadOnlyList<ActionInfo> gameStateActions = _actionProvider.GetGameStateActions(instanceId);
		Action action = null;
		int changeType;
		if (model.IsParentWithTwoFrontFacets())
		{
			bool num = model.IsSplitCard();
			bool flag = model.IsRoomParent();
			ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, _cardDatabase.CardDataProvider);
			ICardDataAdapter cardDataAdapter = ((num || flag) ? model.GetLinkedFaceAtIndex(1, ignoreInstance: false, _cardDatabase.CardDataProvider) : model);
			Action actionForObjectType = GetActionForObjectType(linkedFaceAtIndex.Instance.GetGameObjectType(), gameStateActions, requestActions);
			Action actionForObjectType2 = GetActionForObjectType(cardDataAdapter.Instance.GetGameObjectType(), gameStateActions, requestActions);
			List<ManaQuantity> manaList;
			bool flag2 = ManaUtilities.CalculateCastingCostDifferences(linkedFaceAtIndex, actionForObjectType, out manaList, out changeType);
			List<ManaQuantity> manaList2;
			bool flag3 = ManaUtilities.CalculateCastingCostDifferences(cardDataAdapter, actionForObjectType2, out manaList2, out changeType);
			if (flag2 || flag3)
			{
				action = (flag2 ? actionForObjectType : actionForObjectType2);
				text = $"{ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaText(manaList))}/{ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaText(manaList2))}";
			}
		}
		else
		{
			Action actionForObjectType3 = GetActionForObjectType(model.Instance.GetGameObjectType(), gameStateActions, requestActions);
			if (ManaUtilities.CalculateCastingCostDifferences(model, actionForObjectType3, out var manaList3, out changeType))
			{
				action = actionForObjectType3;
				text = ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaText(manaList3));
			}
		}
		if (_actionOverrideProvider.TryGetHangerConfig(action, out var hangerConfig))
		{
			yield return hangerConfig;
		}
		else if (!string.IsNullOrEmpty(text))
		{
			string localizedText = _locProvider.GetLocalizedText("AbilityHanger/CostChange/CostModified_Header");
			string localizedText2 = _locProvider.GetLocalizedText("AbilityHanger/CostChange/CostModified_Body", ("newCost", text));
			yield return new HangerConfig(localizedText, localizedText2);
		}
	}

	private static bool IsValidObjectType(GameObjectType objectType)
	{
		return objectType switch
		{
			GameObjectType.Ability => false, 
			GameObjectType.Token => false, 
			GameObjectType.Emblem => false, 
			GameObjectType.Boon => false, 
			_ => true, 
		};
	}

	private Action GetActionForObjectType(GameObjectType objectType, IReadOnlyList<ActionInfo> actions, IReadOnlyList<GreInteraction> interactions)
	{
		return _actionPriorityCalculator.GetPrioritizedAction(_cardDatabase.AbilityDataProvider, interactions, actions, ManaUtilities.GetActionTypeFilter(objectType));
	}
}
