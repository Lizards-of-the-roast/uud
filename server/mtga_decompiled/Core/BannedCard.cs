using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class BannedCard : MonoBehaviour
{
	private Meta_CDC _bannedCardInstance;

	[SerializeField]
	private Transform _locatorWildcard;

	[SerializeField]
	private CDCMetaCardView CardView;

	public void CardCleanup()
	{
		if (null != _bannedCardInstance)
		{
			_bannedCardInstance.Teardown();
			Object.Destroy(_bannedCardInstance.gameObject);
		}
	}

	public void ShowBannedCard(uint titleId, MetaCardHolder cardHolder)
	{
		CardDatabase cardDatabase = WrapperController.Instance.CardDatabase;
		CardViewBuilder cardViewBuilder = WrapperController.Instance.CardViewBuilder;
		uint num = 0u;
		CardPrintingData printing = null;
		foreach (CardPrintingData item in cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(titleId))
		{
			if (item.GrpId > num && !cardDatabase.AltPrintingProvider.IsAltPrinting(item.GrpId))
			{
				num = item.GrpId;
				printing = item;
			}
		}
		CardCleanup();
		CardData data = new CardData(null, printing);
		CardView.Init(cardDatabase, cardViewBuilder);
		CardView.SetData(data);
		CardView.Holder = cardHolder;
		Meta_CDC cardView = CardView.CardView;
		cardView.transform.ZeroOut();
		cardView.transform.localScale = new Vector3(1f, 1f, 1f);
		_bannedCardInstance = cardView;
	}
}
