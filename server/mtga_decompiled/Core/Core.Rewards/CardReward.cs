using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using GreClient.CardData;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class CardReward : CardlikeReward<CardAdded, RewardDisplayCard>
{
	protected override RewardType _rewardType => RewardType.Card;

	public override int ToAddCount => ToAdd.Count((CardAdded c) => c.ShouldDisplay());

	public void AddCard(CardAdded newCardAdded)
	{
		if (newCardAdded.count >= 1)
		{
			CardAdded cardAdded = ToAdd.FirstOrDefault((CardAdded x) => x.GrpID == newCardAdded.GrpID);
			if (cardAdded != null)
			{
				cardAdded.count += newCardAdded.count;
			}
			else
			{
				ToAdd.Enqueue(newCardAdded);
			}
		}
	}

	private void AddCard(uint grpId, int count)
	{
		AddCard(new CardAdded
		{
			GrpID = grpId,
			count = (uint)count
		});
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		foreach (CardAdded unpairedCard in cache.UnpairedCards)
		{
			AddCard(unpairedCard);
		}
		AddCard(9u, inventoryUpdate.delta.wcCommonDelta);
		AddCard(8u, inventoryUpdate.delta.wcUncommonDelta);
		AddCard(7u, inventoryUpdate.delta.wcRareDelta);
		AddCard(6u, inventoryUpdate.delta.wcMythicDelta);
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (CardAdded cardAdded in ToAdd.Where((CardAdded cardAdded2) => cardAdded2.ShouldDisplay()))
		{
			yield return (RewardDisplayContext ctxt) => ShowCard(ccr, cardAdded, ctxt.ChildIndex, ctxt.AutoFlipping);
		}
	}

	private IEnumerator ShowCard(ContentControllerRewards ccr, CardAdded cardAdded, int childIndex, bool autoFlipping)
	{
		RewardDisplayCard rewardDisplayCard = Instantiate(ccr, childIndex);
		rewardDisplayCard.gameObject.SetActive(value: false);
		rewardDisplayCard.SetQuantity(cardAdded.count);
		CardData cardData;
		if (cardAdded.IsGemCard())
		{
			cardData = CardDataExtensions.CreateRewardsCard(ccr.CardDatabase, cardAdded.AetherizedInfo.goldAwarded, cardAdded.AetherizedInfo.gemsAwarded, cardAdded.AetherizedInfo.set);
		}
		else if (cardAdded.IsDisplayableCard())
		{
			cardData = new CardData(null, ccr.CardDatabase.CardDataProvider.GetCardPrintingById(cardAdded.GrpID));
		}
		else
		{
			SimpleLog.LogError("Incoming CardAdded which Should Display is unsupported by the ContentControllerRewards");
			cardData = null;
		}
		CDCMetaCardView cDCMetaCardView = CreateCardInstanceInParent(cardData, rewardDisplayCard.CardParent1.transform);
		rewardDisplayCard.card = cDCMetaCardView.CardView;
		rewardDisplayCard.SetRarity(cDCMetaCardView.CardView.Model.Rarity, cardAdded.ExpectedRarity);
		cDCMetaCardView.UpdateNumberNew(cardData?.GrpId ?? 0);
		HandleAutoFlipCard(rewardDisplayCard, autoFlipping, (cardData != null && cardData.IsWildcard) || cardAdded.IsGemCard());
		rewardDisplayCard.gameObject.SetActive(value: true);
		ccr._cardHolder.SetCards(ccr.CardHolderCollection);
		yield return null;
	}
}
