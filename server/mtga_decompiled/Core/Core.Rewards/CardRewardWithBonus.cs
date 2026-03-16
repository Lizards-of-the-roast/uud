using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

namespace Core.Rewards;

[Serializable]
public class CardRewardWithBonus : CardlikeReward<CardAddedWithBonus, RewardDisplayCardWithBonus>
{
	protected override RewardType _rewardType => RewardType.AlchemyCard;

	public override int ToAddCount => ToAdd.Count((CardAddedWithBonus c) => c.card.ShouldDisplay());

	private void AddCardWithBonus(CardAddedWithBonus newCardAdded)
	{
		CardAddedWithBonus cardAddedWithBonus = ToAdd.FirstOrDefault((CardAddedWithBonus x) => x.card.GrpID == newCardAdded.card.GrpID && x.bonusCard.GrpID == newCardAdded.bonusCard.GrpID);
		if (cardAddedWithBonus != null)
		{
			cardAddedWithBonus.card.count += newCardAdded.card.count;
			cardAddedWithBonus.bonusCard.count += newCardAdded.bonusCard.count;
		}
		else
		{
			ToAdd.Enqueue(newCardAdded);
		}
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		foreach (CardAddedWithBonus pairedCard in cache.PairedCards)
		{
			AddCardWithBonus(pairedCard);
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (CardAddedWithBonus cardAddedWithBonus in ToAdd.Where((CardAddedWithBonus c) => c.card.ShouldDisplay() && c.bonusCard.ShouldDisplay()))
		{
			yield return (RewardDisplayContext ctxt) => ShowAlchemyCardPair(ccr, cardAddedWithBonus, ctxt.ChildIndex, ctxt.AutoFlipping);
		}
	}

	private IEnumerator ShowAlchemyCardPair(ContentControllerRewards ccr, CardAddedWithBonus cardAddedWithBonus, int childIndex, bool autoFlipping)
	{
		RewardDisplayCardWithBonus rewardDisplayCardWithBonus = Instantiate(ccr, childIndex);
		rewardDisplayCardWithBonus.gameObject.SetActive(value: false);
		rewardDisplayCardWithBonus.SetCardQuantity(cardAddedWithBonus.card.count);
		CDCMetaCardView cDCMetaCardView = CreateCardInstanceInParent(cardAddedWithBonus.bonusCard.GrpID, rewardDisplayCardWithBonus.CardParent2.transform);
		rewardDisplayCardWithBonus.cardBonus = cDCMetaCardView.CardView;
		CDCMetaCardView cDCMetaCardView2 = CreateCardInstanceInParent(cardAddedWithBonus.card.GrpID, rewardDisplayCardWithBonus.CardParent1.transform);
		rewardDisplayCardWithBonus.card = cDCMetaCardView2.CardView;
		rewardDisplayCardWithBonus.SetRarity(cDCMetaCardView2.CardView.Model.Rarity, cardAddedWithBonus.card.ExpectedRarity);
		UpdateNewTag(cardAddedWithBonus.card.GrpID, rewardDisplayCardWithBonus.newTagObject);
		HandleAutoFlipCard(rewardDisplayCardWithBonus, autoFlipping);
		rewardDisplayCardWithBonus.gameObject.SetActive(value: true);
		ccr._cardHolder.SetCards(ccr.CardHolderCollection);
		yield return null;
	}

	public static void UpdateNewTag(uint grpId, GameObject tabObject)
	{
		if ((bool)WrapperController.Instance && WrapperController.Instance.InventoryManager?.CardsToTagNew != null)
		{
			WrapperController.Instance.InventoryManager.CardsToTagNew.TryGetValue(grpId, out var value);
			if (value == 1 && WrapperController.Instance.InventoryManager.Cards[grpId] == 1)
			{
				tabObject.SetActive(value: true);
				tabObject.GetComponentInChildren<Localize>().SetText("MainNav/NewTags/First_label");
			}
		}
	}
}
