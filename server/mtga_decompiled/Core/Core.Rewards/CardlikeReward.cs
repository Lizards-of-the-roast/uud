using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Core.Rewards;

public abstract class CardlikeReward<T, P> : ItemReward<T, P>, ICardlikeReward, IRewardBase where P : Component
{
	public BoosterMetaCardHolder CardHolder { private get; set; }

	public BoosterMetaCardView CardPrefab { private get; set; }

	public CardCollection CardHolderCollection { private get; set; }

	public CardDatabase CardDatabase { private get; set; }

	public CardViewBuilder CardViewBuilder { private get; set; }

	public ICardlikeReward[] CardlikeRewards { private get; set; }

	protected CDCMetaCardView CreateCardInstanceInParent(uint grpId, Transform metaCardViewParent)
	{
		CardData cardData = new CardData(null, CardDatabase.CardDataProvider.GetCardPrintingById(grpId));
		return CreateCardInstanceInParent(cardData, metaCardViewParent);
	}

	protected CDCMetaCardView CreateCardInstanceInParent(CardData cardData, Transform metaCardViewParent)
	{
		CDCMetaCardView cDCMetaCardView = Object.Instantiate(CardPrefab, metaCardViewParent);
		cDCMetaCardView.InitWithData(cardData, CardDatabase, CardViewBuilder);
		cDCMetaCardView.Holder = CardHolder;
		CardHolderCollection.Add(cDCMetaCardView.Card, 1);
		return cDCMetaCardView;
	}

	protected void HandleAutoFlipCard(RewardDisplayCard cardReward, bool autoFlipping, bool cardAlwaysFlips = false)
	{
		if (autoFlipping || cardAlwaysFlips)
		{
			cardReward.AutoFlip = true;
		}
		AudioManager.PlayAudio(autoFlipping ? WwiseEvents.sfx_ui_main_rewards_wild_flipout : WwiseEvents.sfx_ui_main_rewards_card_flipout, cardReward.gameObject);
	}
}
