using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class SleeveReward : VanityItemReward<string, RewardDisplayCardSleeve>
{
	private static readonly int DISABLED_ANIMATOR_FLAG = Animator.StringToHash("Disabled");

	protected override RewardType _rewardType => RewardType.Sleeve;

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public string ClaimButtonText { private get; set; }

	public override string VanityItemPrefix => "cardbacks";

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (string sleeveName in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowSleeveReward(ccr, sleeveName, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowSleeveReward(ContentControllerRewards ccr, string sleeveName, int childIndex)
	{
		RewardDisplayCardSleeve rewardDisplayCardSleeve = Instantiate(ccr, childIndex);
		CardData data = CardDataExtensions.CreateSkinCard(0u, ccr.CardDatabase, ClaimButtonText, sleeveName);
		CDCMetaCardView cDCMetaCardView = UnityEngine.Object.Instantiate(ccr._cardPrefab, rewardDisplayCardSleeve.CardAnchor);
		cDCMetaCardView.InitWithData(data, ccr.CardDatabase, ccr.CardViewBuilder);
		cDCMetaCardView.Holder = ccr._cardHolder;
		cDCMetaCardView.gameObject.SetActive(value: true);
		cDCMetaCardView.ActivateFirstTag(isFirst: true);
		rewardDisplayCardSleeve.Init(sleeveName, cDCMetaCardView, OnSleeveSetDefault(rewardDisplayCardSleeve));
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_card_flipout, rewardDisplayCardSleeve.gameObject);
		yield return null;
	}

	public Action<string> OnSleeveSetDefault(RewardDisplayCardSleeve sleeveDisplay)
	{
		return OnSleeveSetDefaultInner;
		void OnSleeveSetDefaultInner(string id)
		{
			CosmeticsProvider.SetCardbackSelection(id);
			sleeveDisplay.ApplySleeveButtonAnimator.SetBool(DISABLED_ANIMATOR_FLAG, value: true);
			sleeveDisplay.ApplySleeveButton.Interactable = false;
			sleeveDisplay.UnregisterOnApplyButtonPressed(OnSleeveSetDefaultInner);
		}
	}

	public override void AddVanityItem(string name)
	{
		AddItemIfUnique(name);
	}
}
