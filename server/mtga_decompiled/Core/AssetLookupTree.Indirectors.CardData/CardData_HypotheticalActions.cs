using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_HypotheticalActions : IIndirector
{
	private AbilityPrintingData _abilityCache;

	private ActionType _typeCache;

	private Wotc.Mtgo.Gre.External.Messaging.Action _actionCache;

	public void SetCache(IBlackboard bb)
	{
		_abilityCache = bb.Ability;
		_typeCache = bb.GreActionType;
		_actionCache = bb.GreAction;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _abilityCache;
		bb.GreActionType = _typeCache;
		bb.GreAction = _actionCache;
		_abilityCache = null;
		_typeCache = ActionType.None;
		_actionCache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance == null)
		{
			yield break;
		}
		foreach (ActionInfo action in bb.CardData.Instance.Actions)
		{
			if (action?.Action != null)
			{
				AbilityPrintingData ability = null;
				if (action.Action.AlternativeGrpId != 0)
				{
					ability = bb.AbilityDataProvider.GetAbilityPrintingById(action.Action.AlternativeGrpId);
				}
				else if (action.Action.AbilityGrpId != 0)
				{
					ability = bb.AbilityDataProvider.GetAbilityPrintingById(action.Action.AbilityGrpId);
				}
				bb.Ability = ability;
				bb.GreAction = action.Action;
				bb.GreActionType = action.Action.ActionType;
				yield return bb;
			}
		}
		foreach (MtgCardInstance linkedFaceInstance in bb.CardData.Instance.LinkedFaceInstances)
		{
			foreach (ActionInfo action2 in linkedFaceInstance.Actions)
			{
				if (action2?.Action != null)
				{
					AbilityPrintingData ability2 = null;
					if (action2.Action.AlternativeGrpId != 0)
					{
						ability2 = bb.AbilityDataProvider.GetAbilityPrintingById(action2.Action.AlternativeGrpId);
					}
					else if (action2.Action.AbilityGrpId != 0)
					{
						ability2 = bb.AbilityDataProvider.GetAbilityPrintingById(action2.Action.AbilityGrpId);
					}
					bb.Ability = ability2;
					bb.GreAction = action2.Action;
					bb.GreActionType = action2.Action.ActionType;
					yield return bb;
				}
			}
		}
	}
}
