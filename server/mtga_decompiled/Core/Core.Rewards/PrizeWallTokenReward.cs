using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Code.ClientFeatureToggle;
using Core.Code.PrizeWall;
using Core.Meta.MainNavigation.Rewards;
using Core.Meta.MainNavigation.Store;
using Core.Shared.Code.ClientModels;
using Wizards.Arena.Enums.Store;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class PrizeWallTokenReward : ItemReward<PrizeWallTokenRewardModel, PrizeWallTokenRewardDisplay>
{
	protected override RewardType _rewardType => RewardType.PrizeWallToken;

	private ICustomTokenProvider CustomTokenProvider => Pantry.Get<ICustomTokenProvider>();

	private PrizeWallDataProvider prizeWallDataProvider => Pantry.Get<PrizeWallDataProvider>();

	private ClientFeatureToggleDataProvider _clientFeatureToggleDataProvider => Pantry.Get<ClientFeatureToggleDataProvider>();

	public void AddTokenReward(ICustomTokenProvider eventTokenProvider, string tokenId, int amount, string rewardNavLocKey)
	{
		if (amount > 0)
		{
			ToAdd.Enqueue(ModelForAmount(eventTokenProvider, tokenId, amount));
		}
	}

	private PrizeWallTokenRewardModel ModelForAmount(ICustomTokenProvider customTokenProvider, string tokenId, int amount)
	{
		if (amount <= 0)
		{
			return null;
		}
		Client_CustomTokenDefinition client_CustomTokenDefinition = customTokenProvider.TokenDefinitions[tokenId];
		Client_PrizeWall client_PrizeWall = prizeWallDataProvider.GetPrizeWallsByCurrencyId(client_CustomTokenDefinition.TokenId).FirstOrDefault();
		return new PrizeWallTokenRewardModel
		{
			LookupString = client_CustomTokenDefinition.PrefabName,
			Amount = amount,
			TokenId = client_CustomTokenDefinition.TokenId,
			RewardNavLocKey = client_PrizeWall?.RewardNavLocKey
		};
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		ICustomTokenProvider customTokenProvider = CustomTokenProvider;
		IEnumerable<CustomTokenDeltaInfo> customTokenDelta = inventoryUpdate.delta.customTokenDelta;
		foreach (CustomTokenDeltaInfo item in customTokenDelta ?? Enumerable.Empty<CustomTokenDeltaInfo>())
		{
			if (!customTokenProvider.IsTokenOfType(item.id, ClientTokenType.PrizeWall))
			{
				continue;
			}
			foreach (Client_PrizeWall item2 in prizeWallDataProvider.GetPrizeWallsByCurrencyId(item.id))
			{
				if (ShouldNotifyPrizeWallTokenReward(item2.Id))
				{
					AddTokenReward(customTokenProvider, item.id, item.delta, item2.RewardNavLocKey);
					break;
				}
			}
		}
	}

	private bool ShouldNotifyPrizeWallTokenReward(string prizeWallId)
	{
		if (prizeWallDataProvider.IsPrizeWallUnlocked(prizeWallId))
		{
			return true;
		}
		if (_clientFeatureToggleDataProvider.GetToggleValueById("PrizeWallTixShouldNotify") && !prizeWallDataProvider.WasPrizeWallRefunded(prizeWallId))
		{
			return true;
		}
		return false;
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (PrizeWallTokenRewardModel tokenRewardModel in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowPrizeWallTokenReward(ccr, tokenRewardModel, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowPrizeWallTokenReward(ContentControllerRewards ccr, PrizeWallTokenRewardModel tokenRewardModel, int childIndex)
	{
		PrizeWallTokenRewardDisplay prizeWallTokenRewardDisplay = Instantiate(ccr, childIndex);
		prizeWallTokenRewardDisplay.SetToken(tokenRewardModel, ccr.AssetLookupSystem);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_coins_flipout, prizeWallTokenRewardDisplay.gameObject);
		yield return null;
	}
}
