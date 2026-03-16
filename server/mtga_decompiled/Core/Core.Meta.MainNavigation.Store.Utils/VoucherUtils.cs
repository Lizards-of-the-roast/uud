using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Shared.Code.Utilities;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Store.Utils;

public class VoucherUtils
{
	public static AltAssetReference<TPrefab> VoucherRefForId<TPayload, TPrefab>(AssetLookupSystem assetLookupSystem, string voucherId) where TPayload : PrefabPayload<TPrefab> where TPrefab : Object
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = voucherId;
		AltAssetReference<TPrefab> prefab = assetLookupSystem.GetPrefab<TPayload, TPrefab>();
		SimpleLogUtils.LogErrorIfNull(prefab, "Could not find voucher for lookup: " + voucherId);
		return prefab;
	}

	public static void UpdateVoucherView(BoosterVoucherView voucherView, Client_VoucherDefinition voucherDef, int quantity)
	{
		voucherView.VoucherId = voucherDef.VoucherId;
		voucherView.SetQuantity(quantity);
		voucherView.SetAvailability(voucherDef.LocKey, voucherDef.EndTime);
		voucherView.SetCollation(CollationMappingUtils.FromString(voucherDef.CollationId), CollationMappingUtils.FromStrings(voucherDef.CollationIds));
	}
}
