using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Wrapper.BonusPack;

namespace Core.Rewards;

[Serializable]
public class PackReward : ItemReward<InventoryBooster, NotificationPopupReward>
{
	protected override RewardType _rewardType => RewardType.Pack;

	public void AddBooster(InventoryBooster stack)
	{
		List<InventoryBooster> setBoosters = ToAdd.Where((InventoryBooster b) => b.CollationId == stack.CollationId).ToList();
		if (setBoosters.Count == 1 && setBoosters[0].Count > 3)
		{
			setBoosters[0].Count += stack.Count;
		}
		else if (setBoosters.Count + stack.Count > 3)
		{
			if (setBoosters.Count > 0)
			{
				setBoosters[0].Count = stack.Count + setBoosters.Sum((InventoryBooster b) => b.Count);
				ToAdd = new Queue<InventoryBooster>(ToAdd.Where((InventoryBooster b) => b == setBoosters[0] || b.CollationId != stack.CollationId));
			}
			else
			{
				ToAdd.Enqueue(stack);
			}
		}
		else
		{
			ToAdd.Enqueue(new InventoryBooster
			{
				CollationId = stack.CollationId,
				Count = stack.Count
			});
		}
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		BonusPackManager bonusPackManager = WrapperController.Instance?.BonusPackManager;
		IEnumerable<BoosterStack> boosterDelta = inventoryUpdate.delta.boosterDelta;
		foreach (BoosterStack item in boosterDelta ?? Enumerable.Empty<BoosterStack>())
		{
			if (bonusPackManager == null || !bonusPackManager.ConsumeHiddenPacks(item.collationId.ToString(), item.count))
			{
				AddBooster(item);
			}
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (InventoryBooster inventoryBooster in ToAdd.OrderBy((InventoryBooster p) => p.CollationId))
		{
			yield return (RewardDisplayContext ctxt) => ShowPackReward(ccr, inventoryBooster, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowPackReward(ContentControllerRewards ccr, InventoryBooster boosterStack, int childIndex)
	{
		NotificationPopupReward notificationPopupReward = Instantiate(ccr, childIndex);
		notificationPopupReward.SetCount(boosterStack.Count);
		TempRewardTranslation.LookupBoosterTextures(boosterStack.CollationId, out var bgPath, out var fgPath, ccr.AssetLookupSystem);
		if (!string.IsNullOrEmpty(bgPath))
		{
			notificationPopupReward.SetBackgroundTexture(bgPath);
		}
		if (!string.IsNullOrEmpty(fgPath))
		{
			notificationPopupReward.SetForegroundTexture(fgPath);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_pack_flipout, notificationPopupReward.gameObject);
		yield return null;
	}
}
