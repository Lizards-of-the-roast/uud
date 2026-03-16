using Wotc.Mtga.Cards.Database;

namespace Core.Rewards;

public interface ICardlikeReward : IRewardBase
{
	BoosterMetaCardHolder CardHolder { set; }

	BoosterMetaCardView CardPrefab { set; }

	CardCollection CardHolderCollection { set; }

	CardDatabase CardDatabase { set; }

	CardViewBuilder CardViewBuilder { set; }

	ICardlikeReward[] CardlikeRewards { set; }
}
