using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Rewards;
using Core.Meta.MainNavigation.Store;
using Core.Meta.Tokens;
using Core.Shared.Code.ClientModels;
using Wizards.Arena.Enums.Store;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class EventTokenReward : ItemReward<TokenRewardModel, TokenRewardDisplay>
{
	protected override RewardType _rewardType => RewardType.EventToken;

	private ICustomTokenProvider CustomTokenProvider => Pantry.Get<ICustomTokenProvider>();

	public void AddTokenReward(ICustomTokenProvider customTokenProvider, string tokenId, int amount)
	{
		ToAdd.Enqueue(ModelForAmount(customTokenProvider, tokenId, amount));
	}

	private static TokenRewardModel ModelForAmount(ICustomTokenProvider customTokenProvider, string tokenId, int amount)
	{
		if (amount <= 0)
		{
			return null;
		}
		Client_CustomTokenDefinition client_CustomTokenDefinition = customTokenProvider.TokenDefinitions[tokenId];
		string tokenLocalizationKey = TokenViewUtilities.GetTokenLocalizationKey(client_CustomTokenDefinition.TokenId, client_CustomTokenDefinition.HeaderLocKey, amount);
		return new TokenRewardModel
		{
			LookupString = client_CustomTokenDefinition.PrefabName,
			TitleKey = tokenLocalizationKey,
			DescriptionKey = client_CustomTokenDefinition.DescriptionLocKey,
			Amount = amount
		};
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		ICustomTokenProvider customTokenProvider = CustomTokenProvider;
		IEnumerable<CustomTokenDeltaInfo> customTokenDelta = inventoryUpdate.delta.customTokenDelta;
		foreach (CustomTokenDeltaInfo item in customTokenDelta ?? Enumerable.Empty<CustomTokenDeltaInfo>())
		{
			if (customTokenProvider.IsTokenOfType(item.id, ClientTokenType.Event))
			{
				AddTokenReward(customTokenProvider, item.id, item.delta);
			}
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (TokenRewardModel tokenRewardModel in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowEventTokenReward(ccr, tokenRewardModel, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowEventTokenReward(ContentControllerRewards ccr, TokenRewardModel tokenRewardModel, int childIndex)
	{
		TokenRewardDisplay tokenRewardDisplay = Instantiate(ccr, childIndex);
		tokenRewardDisplay.SetToken(tokenRewardModel, ccr.AssetLookupSystem);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_coins_flipout, tokenRewardDisplay.gameObject);
		yield return null;
	}
}
