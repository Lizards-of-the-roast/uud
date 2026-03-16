using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class TitleReward : VanityItemReward<string, RewardDisplayTitle>
{
	protected override RewardType _rewardType => RewardType.Title;

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public override string VanityItemPrefix => "titles";

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (string titleName in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowTitleReward(ccr, titleName, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowTitleReward(ContentControllerRewards ccr, string titleId, int childIndex)
	{
		RewardDisplayTitle rewardDisplayTitle = Instantiate(ccr, childIndex);
		string locKey = CosmeticsProvider.AvailableTitles.Find((CosmeticTitleEntry t) => t.Id == titleId).LocKey;
		rewardDisplayTitle.Init(titleId, locKey, OnTitleSetDefault());
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_deck_flipout, rewardDisplayTitle.gameObject);
		yield return null;
	}

	public Action<string> OnTitleSetDefault()
	{
		return OnTitleSetDefaultInner;
		void OnTitleSetDefaultInner(string id)
		{
			CosmeticsProvider.SetTitleSelection(id);
		}
	}

	public override void AddVanityItem(string name)
	{
		AddItemIfUnique(name);
	}
}
