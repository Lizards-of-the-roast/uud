using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Store;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Utils;
using Core.Shared.Code.Utilities;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Rewards;

[Serializable]
public class VoucherReward : ItemReward<VoucherRewardModel, RewardDisplayPreOrder>
{
	protected override RewardType _rewardType => RewardType.Voucher;

	public VoucherDataProvider VoucherDataProvider => Pantry.Get<VoucherDataProvider>();

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		IEnumerable<VoucherStack> voucherItemsDelta = inventoryUpdate.delta.voucherItemsDelta;
		foreach (VoucherStack item in voucherItemsDelta ?? Enumerable.Empty<VoucherStack>())
		{
			if (item.count > 0)
			{
				ToAdd.Enqueue(new VoucherRewardModel
				{
					id = item.Id.ToString(),
					Amount = item.count
				});
			}
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (VoucherRewardModel voucherRewardModel in ToAdd)
		{
			Client_VoucherDefinition voucherDef = VoucherDataProvider.VoucherDefinitionForId(voucherRewardModel.id);
			SimpleLogUtils.LogErrorIfNull(voucherDef, "Voucher Definition not found for id " + voucherRewardModel.id);
			yield return (RewardDisplayContext ctxt) => ShowVoucherReward(ccr, voucherDef, ctxt.ChildIndex, voucherRewardModel.Amount);
		}
	}

	private IEnumerator ShowVoucherReward(ContentControllerRewards ccr, Client_VoucherDefinition voucherDef, int childIndex, int quantity)
	{
		AltAssetReference<BoosterVoucherView> altAssetReference = VoucherUtils.VoucherRefForId<VoucherPayload, BoosterVoucherView>(ccr.AssetLookupSystem, voucherDef.PrefabName);
		if (altAssetReference != null)
		{
			RewardDisplayPreOrder rewardDisplayPreOrder = Instantiate(ccr, childIndex);
			VoucherUtils.UpdateVoucherView(rewardDisplayPreOrder.SetSku(altAssetReference), voucherDef, quantity);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_card_flipout, rewardDisplayPreOrder.gameObject);
		}
		yield return null;
	}
}
