using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Rewards;

[Serializable]
public class DeckBoxReward : ItemReward<Guid, MetaDeckView>
{
	protected override RewardType _rewardType => RewardType.DeckBox;

	private IPreconDeckServiceWrapper PreconDeckManager => Pantry.Get<IPreconDeckServiceWrapper>();

	public DeckBoxReward()
	{
		GetUniqueId = (Guid g) => g.ToString();
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		IEnumerable<Guid> decksAdded = inventoryUpdate.delta.decksAdded;
		foreach (Guid item in decksAdded ?? Enumerable.Empty<Guid>())
		{
			AddItemIfUnique(item);
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (Guid deckId in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowDeckBox(ccr, deckId, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowDeckBox(ContentControllerRewards ccr, Guid deckId, int childIndex)
	{
		Promise<Dictionary<Guid, Client_Deck>> request = PreconDeckManager.EnsurePreconDecks();
		yield return request.AsCoroutine();
		if (request.Result.TryGetValue(deckId, out var value))
		{
			MetaDeckView component = Instantiate(ccr, childIndex).GetComponent<MetaDeckView>();
			component.Init(ccr.CardDatabase, ccr.CardViewBuilder, value);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_deck_flipout, component.gameObject);
		}
	}
}
