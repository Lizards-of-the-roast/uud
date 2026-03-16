using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class AvatarReward : VanityItemReward<string, RewardDisplayAvatar>
{
	private static readonly int DISABLED_ANIMATOR_FLAG = Animator.StringToHash("Disabled");

	protected override RewardType _rewardType => RewardType.Avatar;

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public override string VanityItemPrefix => "avatars";

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (string avatarName in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowAvatarReward(ccr, avatarName, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowAvatarReward(ContentControllerRewards ccr, string avatarName, int childIndex)
	{
		RewardDisplayAvatar rewardDisplayAvatar = Instantiate(ccr, childIndex);
		rewardDisplayAvatar.SetAvatar(ccr.AssetLookupSystem, avatarName);
		rewardDisplayAvatar.ApplyAvatarButton.SetActive(value: true);
		rewardDisplayAvatar.OnObjectClicked += OnAvatarSetDefault(rewardDisplayAvatar);
		if (rewardDisplayAvatar.hoverSFX != null)
		{
			AudioManager.PlayAudio(rewardDisplayAvatar.hoverSFX, rewardDisplayAvatar.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_deck_flipout, rewardDisplayAvatar.gameObject);
		}
		yield return null;
	}

	public Action<string> OnAvatarSetDefault(RewardDisplayAvatar avatarDisplay)
	{
		return OnAvatarSetDefaultInner;
		void OnAvatarSetDefaultInner(string id)
		{
			CosmeticsProvider.SetAvatarSelection(id);
			avatarDisplay.ApplyAvatarButtonAnimator.SetBool(DISABLED_ANIMATOR_FLAG, value: true);
			avatarDisplay.ApplyAvatarButton.GetComponent<CustomButton>().Interactable = false;
			avatarDisplay.OnObjectClicked -= OnAvatarSetDefaultInner;
		}
	}

	public override void AddVanityItem(string name)
	{
		AddItemIfUnique(name);
	}
}
