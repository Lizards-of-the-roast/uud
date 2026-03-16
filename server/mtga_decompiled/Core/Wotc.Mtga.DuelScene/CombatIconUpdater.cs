using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class CombatIconUpdater : ICombatIconUpdater
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	public CombatIconUpdater(IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider)
	{
		_gameStateProvider = gameStateProvider;
		_workflowProvider = workflowProvider;
	}

	public void UpdateCombatIcons(IEnumerable<DuelScene_CDC> allCards)
	{
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		if (mtgGameState.CurrentPhase != Phase.Combat)
		{
			return;
		}
		WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
		if (currentWorkflow is DeclareAttackersWorkflow)
		{
			return;
		}
		DeclareBlockersWorkflow declareBlockersWorkflow = currentWorkflow as DeclareBlockersWorkflow;
		foreach (DuelScene_CDC allCard in allCards)
		{
			if (allCard == null || allCard.Model == null)
			{
				continue;
			}
			if (declareBlockersWorkflow != null)
			{
				Blocker blocker = null;
				foreach (KeyValuePair<uint, Blocker> allBlocker in declareBlockersWorkflow.AllBlockers)
				{
					if (allBlocker.Value.BlockerInstanceId == allCard.Model.InstanceId)
					{
						blocker = allBlocker.Value;
						break;
					}
				}
				if (blocker != null)
				{
					CombatBlockState combatBlockState = CombatBlockState.None;
					if (blocker.SelectedAttackerInstanceIds.Count > 0)
					{
						combatBlockState = CombatBlockState.IsBlocking;
					}
					else if (declareBlockersWorkflow.PendingBlockerIds.Count > 0)
					{
						bool flag = false;
						foreach (uint pendingBlockerId in declareBlockersWorkflow.PendingBlockerIds)
						{
							if (pendingBlockerId == allCard.Model.InstanceId)
							{
								flag = true;
								break;
							}
						}
						combatBlockState = (flag ? CombatBlockState.PendingBlock : CombatBlockState.PendingBlockUnselected);
					}
					else
					{
						combatBlockState = CombatBlockState.CanBlock;
					}
					allCard.UpdateCombatIcons(new CombatStateData(combatBlockState));
					continue;
				}
				allCard.UpdateCombatIcons(CombatStateData.NotInCombat);
				if (allCard.Model.Instance.AttackState == AttackState.None)
				{
					continue;
				}
				CombatAttackState combatAttackState = CombatAttackState.None;
				bool flag2 = false;
				foreach (KeyValuePair<uint, Blocker> allBlocker2 in declareBlockersWorkflow.AllBlockers)
				{
					if (allBlocker2.Value.SelectedAttackerInstanceIds.Contains(allCard.Model.InstanceId))
					{
						flag2 = true;
						break;
					}
				}
				combatAttackState = (CombatAttackState)((int)combatAttackState | (flag2 ? 16 : 0));
				if (mtgGameState.AttackInfo.TryGetValue(allCard.InstanceId, out var value))
				{
					combatAttackState = (CombatAttackState)((int)combatAttackState | ((value.AlternativeGrpId == 0) ? 4 : 0));
					combatAttackState = (CombatAttackState)((int)combatAttackState | ((value.AlternativeGrpId == 162) ? 8 : 0));
					combatAttackState = (CombatAttackState)((int)combatAttackState | ((value.AlternativeGrpId == 261) ? 64 : 0));
				}
				allCard.UpdateCombatIcons(new CombatStateData(combatAttackState));
			}
			else if (allCard.Model.Instance.BlockState == BlockState.Blocking)
			{
				allCard.UpdateCombatIcons(new CombatStateData(CombatBlockState.IsBlocking));
			}
			else
			{
				CombatAttackState combatAttackState2 = CombatAttackState.None;
				combatAttackState2 = (CombatAttackState)((int)combatAttackState2 | ((allCard.Model.Instance.BlockState == BlockState.Blocked) ? 16 : 0));
				if (mtgGameState.AttackInfo.TryGetValue(allCard.InstanceId, out var value2))
				{
					combatAttackState2 = (CombatAttackState)((int)combatAttackState2 | ((value2.AlternativeGrpId == 0) ? 4 : 0));
					combatAttackState2 = (CombatAttackState)((int)combatAttackState2 | ((value2.AlternativeGrpId == 162) ? 8 : 0));
					combatAttackState2 = (CombatAttackState)((int)combatAttackState2 | ((value2.AlternativeGrpId == 261) ? 64 : 0));
				}
				allCard.UpdateCombatIcons(new CombatStateData(combatAttackState2));
			}
		}
	}
}
