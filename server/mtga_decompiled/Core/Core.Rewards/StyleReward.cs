using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class StyleReward : ItemReward<ArtSkin, RewardDisplayCard>
{
	protected override RewardType _rewardType => RewardType.Style;

	public StyleFlavorAddon StyleFlavorAddon { private get; set; }

	public void TryAddArtSkinItem(SceneLoader sceneLoader, ArtSkin item)
	{
		if (!(sceneLoader != null) || !sceneLoader.IsCardViewerEnabled)
		{
			AddItemIfUnique(item);
		}
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		IEnumerable<ArtSkin> artSkinsAdded = inventoryUpdate.delta.artSkinsAdded;
		foreach (ArtSkin item in artSkinsAdded ?? Enumerable.Empty<ArtSkin>())
		{
			TryAddArtSkinItem(SceneLoader.GetSceneLoader(), item);
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		Dictionary<ArtSkin, RewardDisplayCard> displayCardForFlavor = null;
		foreach (ArtSkin addedSkin in ToAdd)
		{
			CardPrintingData cardPrintingData = ccr.CardDatabase.DatabaseUtilities.GetPrintingsByArtId(addedSkin.artId).FirstOrDefault();
			if (cardPrintingData == null)
			{
				yield break;
			}
			CardData data = CardDataExtensions.CreateSkinCard(cardPrintingData.GrpId, ccr.CardDatabase, addedSkin.ccv);
			data.IsFakeStyleCard = true;
			string altFlavorTextLocKey;
			bool hasFlavorText = ccr.CardDatabase.AltFlavorTextKeyProvider.TryGetAltFlavorTextKey(data, out altFlavorTextLocKey);
			yield return (RewardDisplayContext ctxt) => ShowStyle(ccr, addedSkin, data, ctxt.ChildIndex, hasFlavorText ? (displayCardForFlavor ?? (displayCardForFlavor = new Dictionary<ArtSkin, RewardDisplayCard>())) : null);
			if (hasFlavorText)
			{
				yield return (RewardDisplayContext ctxt) => ShowFlavorText(ccr, altFlavorTextLocKey, ctxt.ChildIndex, displayCardForFlavor, addedSkin);
			}
		}
	}

	private IEnumerator ShowStyle(ContentControllerRewards ccr, ArtSkin addedSkin, CardData data, int childIndex, Dictionary<ArtSkin, RewardDisplayCard> displayCardForFlavor = null)
	{
		RewardDisplayCard rewardDisplayCard = Instantiate(ccr, childIndex);
		rewardDisplayCard.gameObject.SetActive(value: false);
		CDCMetaCardView cDCMetaCardView = UnityEngine.Object.Instantiate(ccr._cardPrefab, rewardDisplayCard.transform);
		cDCMetaCardView.Init(ccr.CardDatabase, ccr.CardViewBuilder);
		cDCMetaCardView.SetData(data);
		Meta_CDC cardView = cDCMetaCardView.CardView;
		Transform transform = cardView.transform;
		transform.SetParent(rewardDisplayCard.CardParent1.transform, worldPositionStays: false);
		transform.localScale = Vector3.one;
		rewardDisplayCard.card = cardView;
		cDCMetaCardView.Holder = ccr._cardHolder;
		ccr.CardHolderCollection.Add(cDCMetaCardView.Card, 1);
		CardRolloverZoomHandler component = rewardDisplayCard.GetComponent<CardRolloverZoomHandler>();
		component.ZoomView = ccr.ZoomHandler;
		component.Card = cDCMetaCardView.Card;
		component.CardCollider = cardView.GetComponentInChildren<Collider>();
		rewardDisplayCard.AutoFlip = true;
		if (displayCardForFlavor != null)
		{
			displayCardForFlavor[addedSkin] = rewardDisplayCard;
		}
		else
		{
			RevealAll(ccr, rewardDisplayCard);
		}
		yield return null;
	}

	private IEnumerator ShowFlavorText(ContentControllerRewards ccr, string altFlavorTextLocKey, int childIndex, Dictionary<ArtSkin, RewardDisplayCard> displayCardForFlavor, ArtSkin addedSkin)
	{
		RewardDisplayFlavorText rewardDisplayFlavorText = StyleFlavorAddon.Instantiate(ccr, childIndex);
		rewardDisplayFlavorText.gameObject.SetActive(value: false);
		rewardDisplayFlavorText.SetText(altFlavorTextLocKey);
		RewardDisplayCard cardReward = displayCardForFlavor[addedSkin];
		RevealAll(ccr, cardReward, rewardDisplayFlavorText);
		yield return null;
	}

	private void RevealAll(ContentControllerRewards ccr, RewardDisplayCard cardReward, RewardDisplayFlavorText flavorToReveal = null)
	{
		cardReward.gameObject.SetActive(value: true);
		AudioManager.PlayAudio(cardReward.AutoFlip ? WwiseEvents.sfx_ui_main_rewards_wild_flipout : WwiseEvents.sfx_ui_main_rewards_card_flipout, cardReward.gameObject);
		if (flavorToReveal != null)
		{
			flavorToReveal.gameObject.SetActive(value: true);
		}
		ccr._cardHolder.SetCards(ccr.CardHolderCollection);
	}
}
