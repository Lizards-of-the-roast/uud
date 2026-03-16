using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using ReferenceMap;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public static class CardViewUtilities
{
	private enum BlockerStacking
	{
		None,
		CanStack,
		CannotStack
	}

	private static HashSet<ReferenceMap.Reference> _tempReferences = new HashSet<ReferenceMap.Reference>();

	private static bool IsSameInternal(MtgCardInstance lhsInstance, MtgCardInstance rhsInstance, IObjectPool pool, MtgGameState gameState, MapAggregate mapAggregate, ref List<uint> leftReferences, ref List<uint> rightReferences, bool skipBlocks = false)
	{
		if (lhsInstance == null && rhsInstance == null)
		{
			return true;
		}
		if (lhsInstance == null != (rhsInstance == null))
		{
			return false;
		}
		if (!SamePerpetualValues(lhsInstance, rhsInstance))
		{
			return false;
		}
		if (!lhsInstance.TitleId.Equals(rhsInstance.TitleId))
		{
			return false;
		}
		if (!lhsInstance.ObjectType.EnumEquals(rhsInstance.ObjectType))
		{
			return false;
		}
		if (lhsInstance.Controller == null != (rhsInstance.Controller == null) || (lhsInstance.Controller != null && rhsInstance.Controller != null && !lhsInstance.Controller.InstanceId.Equals(rhsInstance.Controller.InstanceId)))
		{
			return false;
		}
		if (lhsInstance.Owner == null != (rhsInstance.Owner == null) || (lhsInstance.Owner != null && rhsInstance.Owner != null && !lhsInstance.Owner.InstanceId.Equals(rhsInstance.Owner.InstanceId)))
		{
			return false;
		}
		if (lhsInstance.Distributions.Count > 0)
		{
			return false;
		}
		if (lhsInstance.IsTapped != rhsInstance.IsTapped)
		{
			return false;
		}
		if (lhsInstance.HasSummoningSickness != rhsInstance.HasSummoningSickness)
		{
			return false;
		}
		if (!lhsInstance.Power.Value.Equals(rhsInstance.Power.Value))
		{
			return false;
		}
		if (!lhsInstance.Toughness.Value.Equals(rhsInstance.Toughness.Value))
		{
			return false;
		}
		if (!lhsInstance.SuppressedPower.Value.Equals(rhsInstance.SuppressedPower.Value))
		{
			return false;
		}
		if (!lhsInstance.SuppressedToughness.Value.Equals(rhsInstance.SuppressedToughness.Value))
		{
			return false;
		}
		if (!lhsInstance.PowerToughnessInverted.Equals(rhsInstance.PowerToughnessInverted))
		{
			return false;
		}
		if (!lhsInstance.Damage.Equals(rhsInstance.Damage))
		{
			return false;
		}
		if (!lhsInstance.Loyalty.Equals(rhsInstance.Loyalty))
		{
			return false;
		}
		if (!lhsInstance.Defense.Equals(rhsInstance.Defense))
		{
			return false;
		}
		if (!lhsInstance.LoyaltyActivationsRemaining.Equals(rhsInstance.LoyaltyActivationsRemaining))
		{
			return false;
		}
		if (!lhsInstance.ChooseXResult.Equals(rhsInstance.ChooseXResult))
		{
			return false;
		}
		if (!lhsInstance.Actions.Count.Equals(rhsInstance.Actions.Count))
		{
			return false;
		}
		if (!lhsInstance.Colors.EnumContainSame(rhsInstance.Colors, pool))
		{
			return false;
		}
		if (!lhsInstance.Supertypes.EnumContainSame(rhsInstance.Supertypes, pool))
		{
			return false;
		}
		if (!lhsInstance.CardTypes.EnumContainSame(rhsInstance.CardTypes, pool))
		{
			return false;
		}
		if (!lhsInstance.Subtypes.EnumContainSame(rhsInstance.Subtypes, pool))
		{
			return false;
		}
		if (!lhsInstance.RemovedSubtypes.EnumContainSame(rhsInstance.RemovedSubtypes, pool))
		{
			return false;
		}
		if (!lhsInstance.Abilities.ContainsSameAbilities(rhsInstance.Abilities, pool))
		{
			return false;
		}
		if (!lhsInstance.Zone.Type.EnumEquals(rhsInstance.Zone.Type))
		{
			return false;
		}
		if (!lhsInstance.Counters.Count.Equals(rhsInstance.Counters.Count))
		{
			return false;
		}
		foreach (KeyValuePair<CounterType, int> counter in lhsInstance.Counters)
		{
			if (!rhsInstance.Counters.TryGetValue(counter.Key, out var value))
			{
				return false;
			}
			if (counter.Value != value)
			{
				return false;
			}
		}
		if (!lhsInstance.AffectorOfQualifications.ContainSame(rhsInstance.AffectorOfQualifications, BattlefieldStackQualificationEqualityComparer.Instance, pool))
		{
			return false;
		}
		if (!lhsInstance.AffectedByQualifications.ContainSame(rhsInstance.AffectedByQualifications, BattlefieldStackQualificationEqualityComparer.Instance, pool))
		{
			return false;
		}
		if (!lhsInstance.LinkedInfoTitleLocIds.SetEquals(rhsInstance.LinkedInfoTitleLocIds))
		{
			return false;
		}
		if (!lhsInstance.LinkedInfoText.ContainSame(rhsInstance.LinkedInfoText, pool))
		{
			return false;
		}
		if (!lhsInstance.AffectorOfLinkInfos.ContainSame(rhsInstance.AffectorOfLinkInfos, pool))
		{
			return false;
		}
		if (!lhsInstance.AffectedByLinkInfos.ContainSame(rhsInstance.AffectedByLinkInfos, pool))
		{
			return false;
		}
		if (!lhsInstance.BoonTriggersInitial.Equals(rhsInstance.BoonTriggersInitial))
		{
			return false;
		}
		if (!lhsInstance.BoonTriggersRemaining.Equals(rhsInstance.BoonTriggersRemaining))
		{
			return false;
		}
		if (!lhsInstance.ActiveAbilityWords.ContainSame(rhsInstance.ActiveAbilityWords, pool))
		{
			return false;
		}
		if (lhsInstance.TargetedBy.Count > 0 || rhsInstance.TargetedBy.Count > 0)
		{
			return false;
		}
		if (!lhsInstance.Targets.ContainSame(rhsInstance.Targets, (MtgEntity x) => x.InstanceId, pool))
		{
			return false;
		}
		if (lhsInstance.EnteredZoneThisTurn != rhsInstance.EnteredZoneThisTurn)
		{
			return false;
		}
		if (!lhsInstance.AttackTargetId.Equals(rhsInstance.AttackTargetId))
		{
			return false;
		}
		if (!lhsInstance.BlockState.EnumEquals(rhsInstance.BlockState))
		{
			return false;
		}
		if (!lhsInstance.AttackState.EnumEquals(rhsInstance.AttackState))
		{
			return false;
		}
		if (lhsInstance.FaceDownState.IsFaceDown && rhsInstance.FaceDownState.IsFaceDown)
		{
			if (!lhsInstance.OverlayGrpId.HasValue || !rhsInstance.OverlayGrpId.HasValue)
			{
				return false;
			}
			if (!lhsInstance.BaseGrpId.Equals(rhsInstance.BaseGrpId))
			{
				return false;
			}
		}
		if (lhsInstance.IsObjectCopy != rhsInstance.IsObjectCopy)
		{
			return false;
		}
		if (lhsInstance.IsObjectCopy && !lhsInstance.BaseGrpId.Equals(rhsInstance.BaseGrpId))
		{
			return false;
		}
		if (lhsInstance.MutationChildren.Count != rhsInstance.MutationChildren.Count)
		{
			return false;
		}
		if (lhsInstance.MutationChildren.Count > 0 && !lhsInstance.MutationChildren.ContainSame(rhsInstance.MutationChildren, (MtgCardInstance x) => x.GrpId, pool))
		{
			return false;
		}
		if (!lhsInstance.CrewedAndSaddledParentIds.SetEquals(rhsInstance.CrewedAndSaddledParentIds))
		{
			return false;
		}
		if (lhsInstance.AttachedWithIds.Count != 0L || rhsInstance.AttachedWithIds.Count != 0L)
		{
			return false;
		}
		if (!lhsInstance.ReplacementEffects.ContainSame(rhsInstance.ReplacementEffects, pool))
		{
			return false;
		}
		if (!lhsInstance.ExhaustedAbilities.ContainSame(rhsInstance.ExhaustedAbilities, pool))
		{
			return false;
		}
		if (!lhsInstance.CastingTimeOptions.ContainSame(rhsInstance.CastingTimeOptions))
		{
			return false;
		}
		if (!PendingEffectOverridesAreEqual(lhsInstance.PendingEffectOverrides, rhsInstance.PendingEffectOverrides))
		{
			return false;
		}
		if (!lhsInstance.GroupedIds.SequenceEqual(rhsInstance.GroupedIds))
		{
			return false;
		}
		if (!lhsInstance.Designations.Exists((DesignationData x) => x.Type == Designation.Solved).Equals(rhsInstance.Designations.Exists((DesignationData y) => y.Type == Designation.Solved)))
		{
			return false;
		}
		if (lhsInstance.IsTemporary != rhsInstance.IsTemporary)
		{
			return false;
		}
		if (gameState == null)
		{
			return true;
		}
		if (!skipBlocks)
		{
			if (!AreBlockingInstancesSimilar(lhsInstance, rhsInstance, pool, gameState, mapAggregate))
			{
				return false;
			}
			if (!AreBlockedByInstancesSimilar(lhsInstance, rhsInstance, pool, gameState, mapAggregate))
			{
				return false;
			}
		}
		foreach (KeyValuePair<uint, AttackInfo> item in gameState.AttackInfo)
		{
			uint targetId = item.Value.TargetId;
			if (targetId == lhsInstance.InstanceId || targetId == rhsInstance.InstanceId)
			{
				return false;
			}
		}
		mapAggregate.GetChildren(lhsInstance.InstanceId, ref leftReferences);
		mapAggregate.GetChildren(rhsInstance.InstanceId, ref rightReferences);
		for (int num = leftReferences.Count - 1; num >= 0; num--)
		{
			MtgCardInstance cardById = gameState.GetCardById(leftReferences[num]);
			if (cardById != null && cardById.Zone.Type == ZoneType.Pending && cardById.ObjectType == GameObjectType.Ability && ManaUtilities.IsProduceManaAbility(cardById.GrpId))
			{
				leftReferences.RemoveAt(num);
			}
		}
		for (int num2 = rightReferences.Count - 1; num2 >= 0; num2--)
		{
			MtgCardInstance cardById2 = gameState.GetCardById(rightReferences[num2]);
			if (cardById2 != null && cardById2.Zone.Type == ZoneType.Pending && cardById2.ObjectType == GameObjectType.Ability && ManaUtilities.IsProduceManaAbility(cardById2.GrpId))
			{
				rightReferences.RemoveAt(num2);
			}
		}
		if (leftReferences.Count != rightReferences.Count)
		{
			return false;
		}
		for (int num3 = 0; num3 < leftReferences.Count; num3++)
		{
			MtgCardInstance cardById3 = gameState.GetCardById(leftReferences[num3]);
			if (cardById3 == null)
			{
				continue;
			}
			bool flag = false;
			for (int num4 = 0; num4 < rightReferences.Count; num4++)
			{
				MtgCardInstance cardById4 = gameState.GetCardById(rightReferences[num4]);
				if (cardById4 != null && IsSame(cardById3, cardById4, pool, gameState, mapAggregate))
				{
					flag = true;
					rightReferences.RemoveAt(num4);
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (gameState.DelayedTriggerAffectees.Count > 0)
		{
			DelayedTriggerData delayedTriggerData = default(DelayedTriggerData);
			DelayedTriggerData delayedTriggerData2 = default(DelayedTriggerData);
			bool flag2 = false;
			bool flag3 = false;
			foreach (DelayedTriggerData delayedTriggerAffectee in gameState.DelayedTriggerAffectees)
			{
				if (!flag2 && delayedTriggerAffectee.AffectedIds.Contains(lhsInstance.InstanceId))
				{
					delayedTriggerData = delayedTriggerAffectee;
					flag2 = true;
				}
				if (!flag3 && delayedTriggerAffectee.AffectedIds.Contains(rhsInstance.InstanceId))
				{
					delayedTriggerData2 = delayedTriggerAffectee;
					flag3 = true;
				}
				if (flag2 && flag3)
				{
					break;
				}
			}
			if (delayedTriggerData.AffectorId != delayedTriggerData2.AffectorId)
			{
				return false;
			}
		}
		mapAggregate.GetTriggered(lhsInstance.InstanceId, ref leftReferences);
		mapAggregate.GetTriggered(rhsInstance.InstanceId, ref rightReferences);
		if (leftReferences.Count != rightReferences.Count)
		{
			return false;
		}
		for (int num5 = 0; num5 < leftReferences.Count; num5++)
		{
			if (!gameState.TryGetCard(leftReferences[num5], out var card))
			{
				continue;
			}
			bool flag4 = false;
			for (int num6 = 0; num6 < rightReferences.Count; num6++)
			{
				if (gameState.TryGetCard(rightReferences[num6], out var card2) && IsSame(card, card2, pool, gameState, mapAggregate))
				{
					flag4 = true;
					rightReferences.RemoveAt(num6);
					break;
				}
			}
			if (!flag4)
			{
				return false;
			}
		}
		mapAggregate.GetLinkedDamageRecipients(lhsInstance.InstanceId, ref leftReferences);
		if (leftReferences.Count != 0)
		{
			return false;
		}
		mapAggregate.GetLinkedDamageRecipients(rhsInstance.InstanceId, ref rightReferences);
		if (rightReferences.Count != 0)
		{
			return false;
		}
		mapAggregate.GetLinkedDamageSources(lhsInstance.InstanceId, ref leftReferences);
		if (leftReferences.Count != 0)
		{
			return false;
		}
		mapAggregate.GetLinkedDamageSources(rhsInstance.InstanceId, ref rightReferences);
		if (rightReferences.Count != 0)
		{
			return false;
		}
		mapAggregate.GetLinkedTo(lhsInstance.InstanceId, ref leftReferences);
		if (leftReferences.Count != 0)
		{
			return false;
		}
		mapAggregate.GetLinkedTo(rhsInstance.InstanceId, ref rightReferences);
		if (rightReferences.Count != 0)
		{
			return false;
		}
		return true;
	}

	public static bool IsSame(MtgCardInstance leftInstance, MtgCardInstance rightInstance, IObjectPool objectPool, MtgGameState gameState, MapAggregate mapAggregate)
	{
		if (objectPool == null)
		{
			objectPool = NullObjectPool.Default;
		}
		List<uint> leftReferences = objectPool.PopObject<List<uint>>();
		List<uint> rightReferences = objectPool.PopObject<List<uint>>();
		bool result = IsSameInternal(leftInstance, rightInstance, objectPool, gameState, mapAggregate, ref leftReferences, ref rightReferences);
		leftReferences.Clear();
		rightReferences.Clear();
		objectPool.PushObject(leftReferences, tryClear: false);
		objectPool.PushObject(rightReferences, tryClear: false);
		return result;
	}

	public static bool AreBlockingInstancesSimilar(MtgCardInstance lhsInstance, MtgCardInstance rhsInstance, IObjectPool pool, MtgGameState gameState, MapAggregate mapAggregate)
	{
		if (lhsInstance.BlockingIds.Count != rhsInstance.BlockingIds.Count)
		{
			return false;
		}
		if (lhsInstance.BlockingIds.Count > 1)
		{
			return false;
		}
		if (lhsInstance.BlockingIds.Count == 1)
		{
			MtgCardInstance cardById = gameState.GetCardById(lhsInstance.BlockingIds[0]);
			MtgCardInstance cardById2 = gameState.GetCardById(rhsInstance.BlockingIds[0]);
			if ((cardById.BlockedByIds.Count > 1 || cardById2.BlockedByIds.Count > 1) && !cardById.BlockedByIds.ContainSame(cardById2.BlockedByIds, pool))
			{
				return false;
			}
			List<uint> leftReferences = pool.PopObject<List<uint>>();
			List<uint> rightReferences = pool.PopObject<List<uint>>();
			bool num = IsSameInternal(cardById, cardById2, pool, gameState, mapAggregate, ref leftReferences, ref rightReferences, skipBlocks: true);
			leftReferences.Clear();
			rightReferences.Clear();
			pool.PushObject(leftReferences, tryClear: false);
			pool.PushObject(rightReferences, tryClear: false);
			if (!num)
			{
				return false;
			}
		}
		return true;
	}

	private static bool PendingEffectOverridesAreEqual(MtgCardInstance.PendingEffectOverrideData lhs, MtgCardInstance.PendingEffectOverrideData rhs)
	{
		if (lhs == null || rhs == null)
		{
			return lhs == null == (rhs == null);
		}
		if (!lhs.Power.Equals(rhs.Power))
		{
			return false;
		}
		if (!lhs.Toughness.Equals(rhs.Toughness))
		{
			return false;
		}
		if (lhs.Counters.Count != rhs.Counters.Count)
		{
			return false;
		}
		foreach (KeyValuePair<CounterType, int> counter in lhs.Counters)
		{
			if (!rhs.Counters.TryGetValue(counter.Key, out var value))
			{
				return false;
			}
			if (counter.Value != value)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreBlockedByInstancesSimilar(MtgCardInstance lhsInstance, MtgCardInstance rhsInstance, IObjectPool pool, MtgGameState gameState, MapAggregate mapAggregate)
	{
		if (lhsInstance.BlockedByIds.Count != rhsInstance.BlockedByIds.Count)
		{
			return false;
		}
		if (lhsInstance.BlockedByIds.Count > 1)
		{
			return false;
		}
		if (lhsInstance.BlockedByIds.Count == 1)
		{
			MtgCardInstance cardById = gameState.GetCardById(lhsInstance.BlockedByIds[0]);
			MtgCardInstance cardById2 = gameState.GetCardById(rhsInstance.BlockedByIds[0]);
			if ((cardById.BlockingIds.Count > 1 || cardById2.BlockingIds.Count > 1) && !cardById.BlockingIds.ContainSame(cardById2.BlockingIds, pool))
			{
				return false;
			}
			List<uint> leftReferences = pool.PopObject<List<uint>>();
			List<uint> rightReferences = pool.PopObject<List<uint>>();
			bool num = IsSameInternal(cardById, cardById2, pool, gameState, mapAggregate, ref leftReferences, ref rightReferences, skipBlocks: true);
			leftReferences.Clear();
			rightReferences.Clear();
			pool.PushObject(leftReferences, tryClear: false);
			pool.PushObject(rightReferences, tryClear: false);
			if (!num)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsSame(CardPrintingData leftPrinting, CardPrintingData rightPrinting, IObjectPool objectPool)
	{
		if (leftPrinting == null && rightPrinting == null)
		{
			return true;
		}
		if (leftPrinting == null != (rightPrinting == null))
		{
			return false;
		}
		if (!leftPrinting.TitleId.Equals(rightPrinting.TitleId))
		{
			return false;
		}
		if (!leftPrinting.Power.Value.Equals(rightPrinting.Power.Value))
		{
			return false;
		}
		if (!leftPrinting.Toughness.Value.Equals(rightPrinting.Toughness.Value))
		{
			return false;
		}
		if (!leftPrinting.Colors.EnumContainSame(rightPrinting.Colors, objectPool))
		{
			return false;
		}
		if (!leftPrinting.Supertypes.EnumContainSame(rightPrinting.Supertypes, objectPool))
		{
			return false;
		}
		if (!leftPrinting.Subtypes.EnumContainSame(rightPrinting.Subtypes, objectPool))
		{
			return false;
		}
		if (!leftPrinting.AbilityIds.ContainSame(rightPrinting.AbilityIds, objectPool))
		{
			return false;
		}
		return true;
	}

	public static bool IsSame(ICardDataAdapter leftCard, ICardDataAdapter rightCard, GameManager gm)
	{
		IObjectPool objectPool = null;
		MtgGameState gameState = null;
		MapAggregate mapAggregate = null;
		if (gm != null)
		{
			gameState = gm.CurrentGameState;
			objectPool = gm.GenericPool;
			mapAggregate = gm.ReferenceMapAggregate;
		}
		return IsSame(leftCard, rightCard, objectPool, gameState, mapAggregate);
	}

	public static bool IsSame(ICardDataAdapter leftCard, ICardDataAdapter rightCard, IObjectPool objectPool, MtgGameState gameState, MapAggregate mapAggregate)
	{
		CardPrintingData printing = leftCard.Printing;
		CardPrintingData printing2 = rightCard.Printing;
		if (!IsSame(printing, printing2, objectPool))
		{
			return false;
		}
		MtgCardInstance instance = leftCard.Instance;
		MtgCardInstance instance2 = rightCard.Instance;
		return IsSame(instance, instance2, objectPool, gameState, mapAggregate);
	}

	public static ZonePair GetFromToZoneForCard(DuelScene_CDC cardView, bool added = true)
	{
		if (!added)
		{
			return new ZonePair(cardView.CurrentCardHolder, cardView.CurrentCardHolder);
		}
		return new ZonePair(cardView.PreviousCardHolder, cardView.CurrentCardHolder);
	}

	public static void GetListOfReferencedCards(DuelScene_CDC cardView, GameManager gameManager, HashSet<uint> outRelatedCards)
	{
		outRelatedCards.Clear();
		_tempReferences.Clear();
		if (gameManager == null || !cardView)
		{
			return;
		}
		ICardDataAdapter model = cardView.Model;
		if (model == null)
		{
			return;
		}
		MtgCardInstance instance = model.Instance;
		if (instance == null)
		{
			return;
		}
		MtgGameState gameState = gameManager.CurrentGameState;
		if (gameState == null)
		{
			return;
		}
		MtgGameState latestGameState = gameManager.LatestGameState;
		if (latestGameState == null)
		{
			return;
		}
		EntityViewManager viewManager = gameManager.ViewManager;
		if (viewManager == null)
		{
			return;
		}
		if (instance.CrewedAndSaddledByIds.Count != 0)
		{
			outRelatedCards.UnionWith(instance.CrewedAndSaddledByIds);
		}
		if (gameState.VisibleCards.TryGetValue(instance.InstanceId, out var value))
		{
			outRelatedCards.UnionWith(value.CrewedAndSaddledByIds);
		}
		if (gameState.VisibleCards.TryGetValue(instance.ParentId, out var value2) && value2.CrewedAndSaddledByIds.Count != 0)
		{
			outRelatedCards.UnionWith(value2.CrewedAndSaddledByIds);
		}
		if (cardView.CurrentCardHolder is PlayerCommandCardHolder playerCommandCardHolder)
		{
			PlayerCommandCardHolder.CardStack stackForCard = playerCommandCardHolder.GetStackForCard(cardView);
			if (stackForCard != null)
			{
				foreach (DuelScene_CDC stackedCard in stackForCard.StackedCards)
				{
					AddMiniCdcToSet(stackedCard);
				}
			}
			if (((IFakeCardViewProvider)viewManager).TryGetFakeCard("Living Breakthrough", out DuelScene_CDC fakeCdc) && fakeCdc == cardView)
			{
				foreach (QualificationData qualification in gameState.Qualifications)
				{
					if (qualification.AbilityId == 147831)
					{
						AddRelatedIdsToSet(qualification.SourceParent);
					}
				}
			}
		}
		uint parentId = instance.ParentId;
		if (parentId != 0)
		{
			outRelatedCards.Add(parentId);
		}
		uint instanceId = instance.InstanceId;
		foreach (ReplacementEffectData replacementEffect in instance.ReplacementEffects)
		{
			if (replacementEffect.SourceIds != null && !replacementEffect.SourceIds.Contains(instanceId))
			{
				foreach (uint sourceId in replacementEffect.SourceIds)
				{
					outRelatedCards.Add(sourceId);
				}
			}
			if (replacementEffect.RecipientIds == null || replacementEffect.RecipientIds.Contains(instanceId))
			{
				continue;
			}
			foreach (uint recipientId in replacementEffect.RecipientIds)
			{
				outRelatedCards.Add(recipientId);
			}
		}
		List<ReplacementEffectData> value3 = null;
		if (latestGameState.ReplacementEffects.TryGetValue(instanceId, out value3) || latestGameState.ReplacementEffects.TryGetValue(instance.ParentId, out value3))
		{
			foreach (ReplacementEffectData item in value3)
			{
				if (item.DamageRedirectionId.HasValue)
				{
					outRelatedCards.Add(item.DamageRedirectionId.Value);
				}
			}
		}
		if (instance.AttachedToId != 0 && viewManager.TryGetCardView(instance.AttachedToId, out var cardView2) && (bool)cardView2 && cardView.CurrentCardHolder != cardView2.CurrentCardHolder)
		{
			outRelatedCards.Add(cardView2.InstanceId);
		}
		if (instance.AttachedWithIds.Count != 0)
		{
			foreach (uint attachedWithId in instance.AttachedWithIds)
			{
				if (viewManager.TryGetCardView(attachedWithId, out var cardView3) && (bool)cardView3 && cardView.CurrentCardHolder != cardView3.CurrentCardHolder)
				{
					outRelatedCards.Add(cardView3.InstanceId);
				}
			}
		}
		foreach (uint targetId in instance.TargetIds)
		{
			outRelatedCards.Add(targetId);
		}
		foreach (uint additionalCostId in instance.AdditionalCostIds)
		{
			outRelatedCards.Add(additionalCostId);
		}
		foreach (QualificationData affectorOfQualification in instance.AffectorOfQualifications)
		{
			if (affectorOfQualification.Type == QualificationType.MayPlay)
			{
				outRelatedCards.Add(affectorOfQualification.AffectedId);
			}
		}
		foreach (QualificationData affectedByQualification in instance.AffectedByQualifications)
		{
			QualificationType type = affectedByQualification.Type;
			if (type == QualificationType.MayPlay || type == QualificationType.CantTurnFaceUp)
			{
				outRelatedCards.Add(affectedByQualification.AffectorId);
				uint parentId2 = gameManager.ReferenceMapAggregate.GetParentId(affectedByQualification.AffectorId);
				if (parentId2 != 0)
				{
					outRelatedCards.Add(parentId2);
				}
			}
		}
		foreach (uint item2 in TheRingRelatedIds(instance, gameState))
		{
			outRelatedCards.Add(item2);
		}
		if (instanceId == 0)
		{
			_tempReferences.Clear();
			return;
		}
		AddDelayedTriggerAffectees(instance.InstanceId, gameState, outRelatedCards);
		foreach (uint triggeredById in gameManager.ReferenceMapAggregate.GetTriggeredByIds(cardView.InstanceId))
		{
			if (triggeredById != 0)
			{
				outRelatedCards.Add(triggeredById);
			}
		}
		IReadOnlyDictionary<uint, List<DisqualificationType>> disqualifiersForId = ActionsAvailableWorkflow.GetDisqualifiersForId(cardView.InstanceId, gameManager.CurrentInteraction);
		if (disqualifiersForId != null && disqualifiersForId.Count > 0)
		{
			foreach (KeyValuePair<uint, List<DisqualificationType>> item3 in disqualifiersForId)
			{
				if (!model.Instance.AttachedWithIds.Contains(item3.Key) && model.Instance.InstanceId != item3.Key)
				{
					outRelatedCards.Add(item3.Key);
				}
			}
		}
		Map referenceMap = gameState.ReferenceMap;
		if (referenceMap.GetReferencing(instanceId, ref _tempReferences))
		{
			foreach (ReferenceMap.Reference tempReference in _tempReferences)
			{
				outRelatedCards.Add(tempReference.B);
			}
		}
		if (referenceMap.GetLinkedTo(instanceId, ref _tempReferences))
		{
			foreach (ReferenceMap.Reference tempReference2 in _tempReferences)
			{
				outRelatedCards.Add(tempReference2.B);
			}
		}
		_tempReferences.Clear();
		void AddMiniCdcToSet(DuelScene_CDC miniCdc)
		{
			for (MtgCardInstance mtgCardInstance = miniCdc.Model.Instance; mtgCardInstance != null; mtgCardInstance = mtgCardInstance.Parent)
			{
				uint instanceId2 = mtgCardInstance.InstanceId;
				uint cardUpdatedId = viewManager.GetCardUpdatedId(instanceId2);
				AddRelatedIdsToSet(instanceId2);
				if (instanceId2 != cardUpdatedId)
				{
					AddRelatedIdsToSet(cardUpdatedId);
				}
			}
			outRelatedCards.Add(miniCdc.InstanceId);
		}
		void AddRelatedIdsToSet(uint cardId)
		{
			if (cardId != 0)
			{
				outRelatedCards.Add(cardId);
				foreach (uint triggeredById2 in gameManager.ReferenceMapAggregate.GetTriggeredByIds(cardView.InstanceId))
				{
					if (triggeredById2 != 0)
					{
						outRelatedCards.Add(triggeredById2);
					}
				}
				foreach (TargetSpec item4 in gameState.TargetInfo.FindAll((TargetSpec x) => x.Affector == cardId))
				{
					foreach (uint item5 in item4.Affected)
					{
						outRelatedCards.Add(item5);
						outRelatedCards.Add(viewManager.GetCardUpdatedId(item5));
					}
				}
				AddDelayedTriggerAffectees(cardId, gameState, outRelatedCards);
			}
		}
	}

	private static IEnumerable<uint> TheRingRelatedIds(MtgCardInstance card, MtgGameState gameState)
	{
		if (isTheRing(card))
		{
			uint num = ringBearerForPlayerId(card.Controller?.InstanceId ?? 0, gameState);
			if (num != 0)
			{
				yield return num;
			}
		}
		if (cardIsRingBearer(card))
		{
			uint num2 = ringInstanceIdForPlayer(card.Controller?.InstanceId ?? 0, gameState);
			if (num2 != 0)
			{
				yield return num2;
			}
		}
		static bool cardIsRingBearer(MtgCardInstance mtgCardInstance)
		{
			return mtgCardInstance.Designations?.Exists((DesignationData x) => x.Type == Designation.Ringbearer) ?? false;
		}
		static bool isTheRing(MtgCardInstance instance)
		{
			if (instance.ObjectType == GameObjectType.Emblem)
			{
				return instance.ObjectSourceGrpId == 87496;
			}
			return false;
		}
		static uint ringBearerForPlayerId(uint playerId, MtgGameState mtgGameState)
		{
			foreach (MtgCardInstance visibleCard in mtgGameState.Battlefield.VisibleCards)
			{
				if (visibleCard != null && visibleCard.Controller != null && visibleCard.Controller.InstanceId == playerId && visibleCard.Designations.Exists((DesignationData x) => x.Type == Designation.Ringbearer))
				{
					return visibleCard.InstanceId;
				}
			}
			return 0u;
		}
		static uint ringInstanceIdForPlayer(uint playerId, MtgGameState mtgGameState)
		{
			foreach (MtgCardInstance visibleCard2 in mtgGameState.Command.VisibleCards)
			{
				if (visibleCard2 != null && visibleCard2.Controller != null && visibleCard2.ObjectType == GameObjectType.Emblem && visibleCard2.ObjectSourceGrpId == 87496 && visibleCard2.Controller.InstanceId == playerId)
				{
					return visibleCard2.InstanceId;
				}
			}
			return 0u;
		}
	}

	private static void AddDelayedTriggerAffectees(uint instanceId, MtgGameState gameState, HashSet<uint> relatedCards)
	{
		if (gameState.DelayedTriggerAffectees.Count <= 0)
		{
			return;
		}
		DelayedTriggerData delayedTriggerData = gameState.DelayedTriggerAffectees.Find((DelayedTriggerData x) => x.AffectorId == instanceId);
		if (delayedTriggerData.AffectorId == 0)
		{
			return;
		}
		foreach (uint affectedId in delayedTriggerData.AffectedIds)
		{
			relatedCards.Add(affectedId);
		}
	}

	public static bool ShouldAddedAbilityAppearAdded(AbilityPrintingData abilityPrintingData, MtgCardInstance cardInstance, ICardDatabaseAdapter cardDatabase, out bool isMutation)
	{
		isMutation = false;
		if (abilityPrintingData == null || cardInstance == null || cardDatabase == null)
		{
			return true;
		}
		if (cardInstance.MutationChildren.Count == 0)
		{
			return true;
		}
		uint abilityId = abilityPrintingData.Id;
		if (!cardInstance.MutationChildren.Exists((MtgCardInstance x) => x.Abilities.Exists((AbilityPrintingData y) => y.Id == abilityId)))
		{
			CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(cardInstance.BaseGrpId);
			if (cardPrintingById == null || !cardPrintingById.Abilities.Exists((AbilityPrintingData y) => y.Id == abilityId))
			{
				return true;
			}
		}
		isMutation = true;
		return false;
	}

	public static HashSet<CDCFillerBase> GatherCDCFillers(GameObject root)
	{
		return GatherComponentsOfType<CDCFillerBase>(root);
	}

	private static HashSet<T> GatherComponentsOfType<T>(GameObject root)
	{
		HashSet<T> components = new HashSet<T>();
		GatherFromObjectAndChildren(root);
		return components;
		void GatherFromObjectAndChildren(GameObject gameObject)
		{
			T component = gameObject.GetComponent<T>();
			if (component != null)
			{
				components.Add(component);
			}
			Transform transform = gameObject.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (!child.GetComponent<CDCPart>())
				{
					GatherFromObjectAndChildren(child.gameObject);
				}
			}
		}
	}

	public static void ReplaceMaterialData(CardMaterialBuilder cardMaterialBuilder, ICardDatabaseAdapter cardDatabase, Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData> matReplaceData, ICardDataAdapter model, CardHolderType cardHolderType, Func<MtgGameState> getCurrentGameState, bool dimmed, bool invertColor = false, string primaryArtSuffix = null, string secondaryArtSuffix = null, string cropFormatName = null, Vector2 artOffset = default(Vector2), bool mousedOver = false)
	{
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> matReplaceDatum in matReplaceData)
		{
			MaterialReplacementData value = matReplaceDatum.Value;
			AltReferencedMaterialAndBlock materialReplacement = cardMaterialBuilder.GetMaterialReplacement(cardDatabase, value.Original, value.OriginalTextureName, value.OverrideType, model, cardHolderType, getCurrentGameState, dimmed, dissolve: false, invertColor, primaryArtSuffix, secondaryArtSuffix, cropFormatName, artOffset, mousedOver);
			if (!(value.Override?.SharedMaterial) || materialReplacement.GetHashCode() != value.Override?.GetHashCode())
			{
				value.UpdateOverride(materialReplacement);
			}
			else
			{
				cardMaterialBuilder.DecrementReferenceCount(materialReplacement.BlockHashCode);
			}
		}
	}

	private static bool IsCrewedVehicle(ICardDataAdapter cardData)
	{
		if (cardData.Subtypes.Contains(SubType.Vehicle))
		{
			return cardData.Instance.CrewedAndSaddledByIds.Count > 0;
		}
		return false;
	}

	private static bool StackableTemporaryTypes(ICardDataAdapter lhs, ICardDataAdapter rhs)
	{
		List<LayeredEffectData> layeredEffects = lhs.Instance.LayeredEffects;
		List<LayeredEffectData> layeredEffects2 = rhs.Instance.LayeredEffects;
		if (layeredEffects == null && layeredEffects2 == null)
		{
			return true;
		}
		foreach (LayeredEffectData item in layeredEffects)
		{
			if (!(item.Type == "TemporaryType"))
			{
				continue;
			}
			foreach (LayeredEffectData item2 in layeredEffects2)
			{
				if (item2.Type == "TemporaryType" && item2.SourceAbilityId == item.SourceAbilityId)
				{
					return true;
				}
			}
			return false;
		}
		foreach (LayeredEffectData item3 in layeredEffects2)
		{
			if (item3.Type == "TemporaryType")
			{
				return false;
			}
		}
		return true;
	}

	private static bool CantStackWithAnything(ICardDataAdapter cardData)
	{
		if (cardData.Supertypes.Contains(SuperType.Legendary))
		{
			return true;
		}
		if (cardData.Abilities.ContainsId(170557u) && !cardData.Controller.IsLocalPlayer)
		{
			return true;
		}
		if (cardData.Abilities.ContainsId(173672u) && !cardData.Controller.IsLocalPlayer)
		{
			return true;
		}
		if (cardData.Instance.Designations.Exists((DesignationData x) => x.Type == Designation.Saddled))
		{
			return true;
		}
		return false;
	}

	public static bool CanStack(DuelScene_CDC stackParent, DuelScene_CDC card, IObjectPool objectPool, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ICardViewProvider cardViewProvider, MapAggregate mapAggregate, UIManager uiManager)
	{
		MtgGameState mtgGameState = gameStateProvider.CurrentGameState;
		MtgGameState mtgGameState2 = gameStateProvider.LatestGameState;
		WorkflowBase currentWorkflow = workflowProvider.GetCurrentWorkflow();
		ICardDataAdapter model = stackParent.Model;
		ICardDataAdapter model2 = card.Model;
		if (CantStackWithAnything(model))
		{
			return false;
		}
		if (!model.Instance.SelectedBy.SetEquals(model2.Instance.SelectedBy))
		{
			return false;
		}
		if (!IsSame(model, model2, objectPool, mtgGameState, mapAggregate))
		{
			return false;
		}
		if (!currentWorkflow.CanStack(model, model2))
		{
			return false;
		}
		BlockerStacking blockerStacking = BlockerStacking.None;
		foreach (KeyValuePair<uint, AttackInfo> item in mtgGameState.AttackInfo)
		{
			List<uint> list = objectPool.PopObject<List<uint>>();
			foreach (OrderedDamageAssignment orderedBlocker in item.Value.OrderedBlockers)
			{
				list.Add(orderedBlocker.InstanceId);
			}
			if (list.Contains(model.InstanceId) && list.Contains(model2.InstanceId))
			{
				List<List<uint>> list2 = OrderBlockersByLikeness(list, objectPool, cardViewProvider, mtgGameState, mapAggregate);
				blockerStacking = BlockerStacking.CannotStack;
				foreach (List<uint> item2 in list2)
				{
					if (item2.Contains(model.InstanceId) && item2.Contains(model2.InstanceId))
					{
						blockerStacking = BlockerStacking.CanStack;
						break;
					}
				}
				foreach (List<uint> item3 in list2)
				{
					item3.Clear();
					objectPool.PushObject(item3, tryClear: false);
				}
				list2.Clear();
				objectPool.PushObject(list2, tryClear: false);
			}
			list.Clear();
			objectPool.PushObject(list, tryClear: false);
			if (blockerStacking == BlockerStacking.CanStack)
			{
				break;
			}
		}
		switch (blockerStacking)
		{
		case BlockerStacking.CanStack:
			return true;
		case BlockerStacking.CannotStack:
			return false;
		default:
		{
			if (mtgGameState2.AttackInfo.TryGetValue(model.InstanceId, out var value) && mtgGameState2.AttackInfo.TryGetValue(model2.InstanceId, out var value2) && value.AlternativeGrpId != value2.AlternativeGrpId)
			{
				return false;
			}
			if (model.Instance.IsDamagedThisTurn != model2.Instance.IsDamagedThisTurn)
			{
				return false;
			}
			if (IsCrewedVehicle(model) || IsCrewedVehicle(model2))
			{
				return false;
			}
			if (!StackableTemporaryTypes(model, model2))
			{
				return false;
			}
			DuelScene_CDC draggedCard = CardDragController.DraggedCard;
			DuelScene_CDC hoveredCard = CardHoverController.HoveredCard;
			bool flag = uiManager.ConfirmWidget?.IsOpen ?? false;
			if (stackParent != null && !flag && (draggedCard != null || hoveredCard == null || (hoveredCard.CurrentCardHolder.CardHolderType != CardHolderType.Hand && hoveredCard.CurrentCardHolder.CardHolderType != CardHolderType.Battlefield)))
			{
				HighlightType highlightType = stackParent.CurrentHighlight();
				HighlightType highlightType2 = card.CurrentHighlight();
				if ((highlightType == HighlightType.AutoPay || highlightType2 == HighlightType.AutoPay) && highlightType != highlightType2)
				{
					return false;
				}
			}
			return true;
		}
		}
	}

	private static List<List<uint>> OrderBlockersByLikeness(List<uint> blockerIds, IObjectPool objectPool, ICardViewProvider cardViewProvider, MtgGameState gameState, MapAggregate mapAggregate)
	{
		bool flag = false;
		List<List<uint>> list = objectPool.PopObject<List<List<uint>>>() ?? new List<List<uint>>();
		List<uint> list2 = objectPool.PopObject<List<uint>>() ?? new List<uint>();
		ICardDataAdapter cardDataAdapter = null;
		foreach (uint blockerId in blockerIds)
		{
			if (!cardViewProvider.TryGetCardView(blockerId, out var cardView))
			{
				flag = true;
				continue;
			}
			if (cardDataAdapter != null)
			{
				if (IsSame(cardDataAdapter, cardView.Model, objectPool, gameState, mapAggregate))
				{
					list2.Add(cardView.InstanceId);
				}
				else
				{
					List<uint> list3 = objectPool.PopObject<List<uint>>();
					list3.Clear();
					list3.AddRange(list2);
					list.Add(list3);
					list2.Clear();
					cardDataAdapter = null;
				}
			}
			if (cardDataAdapter == null)
			{
				cardDataAdapter = cardView.Model;
				list2.Add(cardView.InstanceId);
			}
		}
		if (list2.Count > 0)
		{
			List<uint> list4 = objectPool.PopObject<List<uint>>() ?? new List<uint>();
			list4.Clear();
			list4.AddRange(list2);
			list.Add(list4);
			list2.Clear();
		}
		if (flag)
		{
			Debug.LogErrorFormat("{0} null CDCs in the list of ordered blockers.\n{1}", flag, string.Join("\n", blockerIds.ConvertAll((uint x) => $"{x} -> {gameState.GetCardById(x)} -> {cardViewProvider.GetCardView(x)}")));
		}
		list2.Clear();
		objectPool.PushObject(list2, tryClear: false);
		return list;
	}

	public static MtgCardInstance InstanceForCardView(BASE_CDC card)
	{
		if (card == null)
		{
			return null;
		}
		if (card.Model == null)
		{
			return null;
		}
		return card.Model.Instance;
	}

	public static bool SamePerpetualValues(MtgCardInstance cardA, MtgCardInstance cardB)
	{
		if (cardA == cardB)
		{
			return true;
		}
		if (cardA != null != (cardB != null))
		{
			return false;
		}
		if (PerpetualPowerModValue(cardA.LayeredEffects) == PerpetualPowerModValue(cardB.LayeredEffects) && PerpetualPowerSetValue(cardA.LayeredEffects) == PerpetualPowerSetValue(cardB.LayeredEffects) && PerpetualToughnessModValue(cardA.LayeredEffects) == PerpetualToughnessModValue(cardB.LayeredEffects) && PerpetualToughnessSetValue(cardA.LayeredEffects) == PerpetualToughnessSetValue(cardB.LayeredEffects))
		{
			return PerpetualIntensitySetValue(cardA.Designations) == PerpetualIntensitySetValue(cardB.Designations);
		}
		return false;
	}

	public static int? PerpetualPowerSetValue(List<LayeredEffectData> layeredEffects)
	{
		if (layeredEffects == null)
		{
			return null;
		}
		for (int num = layeredEffects.Count - 1; num >= 0; num--)
		{
			LayeredEffectData layeredEffect = layeredEffects[num];
			if (layeredEffect.IsPerpetualPowerSet() && layeredEffect.Details.TryGetValue<int>("perpetualPowerSet", out var value))
			{
				return value;
			}
		}
		return null;
	}

	public static int? PerpetualPowerModValue(List<LayeredEffectData> layeredEffects)
	{
		if (layeredEffects == null || !layeredEffects.Exists((LayeredEffectData x) => x.IsPerpetualPowerMod()))
		{
			return null;
		}
		int num = 0;
		foreach (LayeredEffectData layeredEffect in layeredEffects)
		{
			if (layeredEffect.IsPerpetualPowerMod() && layeredEffect.Details.TryGetValue<int>("perpetualPowerMod", out var value))
			{
				num += value;
			}
		}
		return num;
	}

	public static int? PerpetualToughnessSetValue(List<LayeredEffectData> layeredEffects)
	{
		if (layeredEffects == null)
		{
			return null;
		}
		for (int num = layeredEffects.Count - 1; num >= 0; num--)
		{
			LayeredEffectData layeredEffect = layeredEffects[num];
			if (layeredEffect.IsPerpetualToughnessSet() && layeredEffect.Details.TryGetValue<int>("perpetualToughnessSet", out var value))
			{
				return value;
			}
		}
		return null;
	}

	public static int? PerpetualToughnessModValue(List<LayeredEffectData> layeredEffects)
	{
		if (layeredEffects == null || !layeredEffects.Exists((LayeredEffectData x) => x.IsPerpetualToughnessMod()))
		{
			return null;
		}
		int num = 0;
		foreach (LayeredEffectData layeredEffect in layeredEffects)
		{
			if (layeredEffect.IsPerpetualToughnessMod() && layeredEffect.Details.TryGetValue<int>("perpetualToughnessMod", out var value))
			{
				num += value;
			}
		}
		return num;
	}

	public static int? PerpetualIntensitySetValue(List<DesignationData> designations)
	{
		if (designations == null)
		{
			return null;
		}
		foreach (DesignationData designation in designations)
		{
			if (designation.Type == Designation.Intensity && designation.Value.HasValue)
			{
				return (int)designation.Value.Value;
			}
		}
		return null;
	}

	public static bool ContainsSameAbilities(this IReadOnlyList<AbilityPrintingData> lhs, IReadOnlyList<AbilityPrintingData> rhs, IObjectPool pool)
	{
		if (lhs == null || rhs == null)
		{
			if (lhs == null)
			{
				return rhs == null;
			}
			return false;
		}
		if (pool == null)
		{
			pool = NullObjectPool.Default;
		}
		List<AbilityPrintingData> list = pool.PopObject<List<AbilityPrintingData>>() ?? new List<AbilityPrintingData>();
		list.Clear();
		list.AddRange(rhs);
		HashSet<uint> hashSet = pool.PopObject<HashSet<uint>>() ?? new HashSet<uint>();
		hashSet.Clear();
		bool flag = false;
		for (int num = lhs.Count - 1; num >= 0; num--)
		{
			AbilityPrintingData abilityPrintingData = lhs[num];
			int num2 = -1;
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				if (abilityPrintingData.Id == list[num3].Id)
				{
					num2 = num3;
					break;
				}
			}
			if (num2 >= 0)
			{
				if (abilityPrintingData.GetOmitDuplicates())
				{
					hashSet.Add(abilityPrintingData.Id);
				}
				list.RemoveAt(num2);
			}
			else if (!hashSet.Contains(abilityPrintingData.Id))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			foreach (AbilityPrintingData item in list)
			{
				if (!hashSet.Contains(item.Id))
				{
					flag = true;
					break;
				}
			}
		}
		list.Clear();
		hashSet.Clear();
		pool.PushObject(list, tryClear: false);
		pool.PushObject(hashSet, tryClear: false);
		return !flag;
	}
}
