using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;

public class StoreDisplayExploreBundle : StoreItemDisplay
{
	[Serializable]
	public class PremiumCardData
	{
		public uint grpID;
	}

	[SerializeField]
	private StoreCardView[] _premiumCardPreviews;

	[SerializeField]
	private List<PremiumCardData> _premiumCardData;

	private void Awake()
	{
		if (_premiumCardPreviews.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _premiumCardPreviews.Length; i++)
		{
			if (_premiumCardData.Count > i)
			{
				CardData data = CardDataExtensions.CreateSkinCard(_premiumCardData[i].grpID, WrapperController.Instance.CardDatabase, "DA");
				_premiumCardPreviews[i].gameObject.SetActive(value: true);
				_premiumCardPreviews[i].CreateCard(data, WrapperController.Instance.CardDatabase, WrapperController.Instance.CardViewBuilder);
			}
			else
			{
				_premiumCardPreviews[i].gameObject.SetActive(value: false);
			}
		}
	}
}
