using System;
using System.Collections;
using System.Collections.Generic;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Store;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class EmoteReward : VanityItemReward<string, RewardDisplayEmote>
{
	private const string EQUIP_EMOTE_FAILURE_HEADER = "MainNav/Rewards/EquipEmotesError_Header";

	private const string EQUIP_EMOTE_FAILURE_DESCRIPTION = "MainNav/Rewards/EquipEmotesError_Description";

	protected override RewardType _rewardType => RewardType.Emote;

	private IBILogger BILogger => Pantry.Get<IBILogger>();

	private IEmoteDataProvider EmoteDataProvider => Pantry.Get<IEmoteDataProvider>();

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public override string VanityItemPrefix => "emotes";

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (string emoteName in ToAdd)
		{
			if (EmoteDataProvider.TryGetEmoteData(emoteName, out var emoteData))
			{
				yield return (RewardDisplayContext ctxt) => ShowEmoteReward(ccr, emoteName, emoteData, ctxt.ChildIndex);
			}
		}
	}

	private IEnumerator ShowEmoteReward(ContentControllerRewards ccr, string emoteName, EmoteData emoteData, int childIndex)
	{
		RewardDisplayEmote.EquipState equipState = RewardDisplayEmote.EquipState.Unequipped;
		List<string> playerEmoteSelections = CosmeticsProvider.PlayerEmoteSelections;
		if (playerEmoteSelections.Contains(emoteName))
		{
			equipState = RewardDisplayEmote.EquipState.Equipped;
		}
		else
		{
			EmotePage page = emoteData.Entry.Page;
			IEmoteDataProvider emoteDataProvider = EmoteDataProvider;
			ICollection<string> equippedEmotes = playerEmoteSelections;
			if (EmoteUtils.IsEmoteEquipCapReached(page, emoteDataProvider, in equippedEmotes))
			{
				equipState = RewardDisplayEmote.EquipState.CapReached;
			}
		}
		RewardDisplayEmote rewardDisplayEmote = Instantiate(ccr, childIndex);
		rewardDisplayEmote.Initialize(emoteName, ccr.AssetLookupSystem, equipState, emoteData);
		rewardDisplayEmote.OnObjectClicked += OnEmoteClaimed_Unity(ccr, rewardDisplayEmote);
		if (rewardDisplayEmote.HoverSFX != null)
		{
			AudioManager.PlayAudio(rewardDisplayEmote.HoverSFX, rewardDisplayEmote.gameObject);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_deck_flipout, rewardDisplayEmote.gameObject);
		}
		yield return null;
	}

	public Action<EmoteData, Action, Action> OnEmoteClaimed_Unity(ContentControllerRewards ccr, RewardDisplayEmote emoteDisplay)
	{
		return OnEmoteClaimedUnityInner;
		void OnEmoteClaimedUnityInner(EmoteData emoteData, Action onEquipCapReached, Action onEmoteEquipped)
		{
			emoteDisplay.OnObjectClicked -= OnEmoteClaimedUnityInner;
			List<string> equippedEmotes = CosmeticsProvider.PlayerEmoteSelections;
			if (!equippedEmotes.Contains(emoteData.Id))
			{
				EmotePage page = emoteData.Entry.Page;
				IEmoteDataProvider emoteDataProvider = EmoteDataProvider;
				ICollection<string> equippedEmotes2 = equippedEmotes;
				if (EmoteUtils.IsEmoteEquipCapReached(page, emoteDataProvider, in equippedEmotes2))
				{
					onEquipCapReached?.Invoke();
				}
				else
				{
					string[] previousSelection = new string[equippedEmotes.Count];
					equippedEmotes.CopyTo(previousSelection);
					equippedEmotes.Add(emoteData.Id);
					CosmeticsProvider.SetEmoteSelections(equippedEmotes).ThenOnMainThread(delegate(Promise<PreferredCosmetics> p)
					{
						if (p.Successful)
						{
							onEmoteEquipped?.Invoke();
						}
						else
						{
							SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EquipEmotesError_Header"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EquipEmotesError_Description"));
						}
						BILogger.Send(ClientBusinessEventType.EmoteSelectionsModified, new EmoteSelectionsModified
						{
							SelectionMethod = "Reward",
							SaveSuccessful = p.Successful,
							PreviousSelectedEmotes = previousSelection,
							UpdatedSelectedEmotes = equippedEmotes.ToArray(),
							EventTime = DateTime.UtcNow
						});
					});
				}
			}
		}
	}

	public override void AddVanityItem(string name)
	{
		AddItemIfUnique(name);
	}
}
