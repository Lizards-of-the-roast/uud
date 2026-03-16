using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using DG.Tweening;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class ListMetaCardHolder_Expanding : MetaCardHolder
{
	public bool ButtonsAreInteractable = true;

	public bool DisableAddButtonOnly;

	[SerializeField]
	private Transform _cardParent;

	[SerializeField]
	private ListMetaCardView_Expanding _cardPrefab;

	[SerializeField]
	private ListCardViewAnimationData _animationData;

	[SerializeField]
	private FrameSpriteData[] _frameSpritesData;

	private List<ListMetaCardView_Expanding> _tiles = new List<ListMetaCardView_Expanding>();

	[SerializeField]
	private Scrollbar _scrollbar;

	[SerializeField]
	public ListCommanderHolder _commanderHolder;

	[SerializeField]
	public ListCommanderHolder _partnerHolder;

	[SerializeField]
	public ListCommanderHolder _companionHolder;

	public Action<MetaCardView> OnCardAddClicked { get; set; }

	public Action<MetaCardView> OnCardRemoveClicked { get; set; }

	public List<ListMetaCardView_Expanding> CardViews => _tiles;

	public void SetCards(List<ListMetaCardViewDisplayInformation> newDisplayInfo, CardFilter textCardFilter = null)
	{
		newDisplayInfo = newDisplayInfo.Where((ListMetaCardViewDisplayInformation x) => x.Quantity != 0).ToList();
		for (int num = _tiles.Count - 1; num >= 0; num--)
		{
			ListMetaCardView_Expanding listMetaCardView_Expanding = _tiles[num];
			bool flag = true;
			foreach (ListMetaCardViewDisplayInformation item2 in newDisplayInfo)
			{
				if (listMetaCardView_Expanding.Card.GrpId == item2.Card.GrpId && listMetaCardView_Expanding.SkinCode == item2.SkinCode && listMetaCardView_Expanding.ShowBannedTreatment == item2.Banned && listMetaCardView_Expanding.ShowUnCollectedTreatment == item2.Unowned)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ReleaseCardView(listMetaCardView_Expanding);
				_tiles.RemoveAt(num);
			}
		}
		int num2 = 0;
		foreach (ListMetaCardViewDisplayInformation item3 in newDisplayInfo)
		{
			if (num2 < _tiles.Count)
			{
				ListMetaCardView_Expanding listMetaCardView_Expanding2 = _tiles[num2];
				if (listMetaCardView_Expanding2.Card.GrpId == item3.Card.GrpId && listMetaCardView_Expanding2.SkinCode == item3.SkinCode && listMetaCardView_Expanding2.ShowBannedTreatment == item3.Banned && listMetaCardView_Expanding2.ShowUnCollectedTreatment == item3.Unowned)
				{
					listMetaCardView_Expanding2.ShowInvalidTreatment = item3.Invalid;
					listMetaCardView_Expanding2.HangerSituation = new HangerSituation
					{
						ContextualHangers = item3.ContextualHangers
					};
					listMetaCardView_Expanding2.SetQuantity((int)item3.Quantity);
					num2++;
					continue;
				}
			}
			CardData cardData = CardDataExtensions.CreateSkinCard(item3.Card.GrpId, base.CardDatabase, item3.SkinCode);
			ListMetaCardView_Expanding item = CreateCardView(cardData, item3);
			_tiles.Insert(num2, item);
			num2++;
		}
		for (int num3 = 0; num3 < _tiles.Count; num3++)
		{
			_tiles[num3].transform.SetSiblingIndex(num3);
		}
	}

	private void ReleaseCardView(ListMetaCardView_Expanding cardView)
	{
		if ((bool)cardView)
		{
			cardView.Cleanup();
			UnityEngine.Object.Destroy(cardView.gameObject);
		}
	}

	public void ScrollToGrpId(uint grpId)
	{
		if (grpId != 0)
		{
			float size = _scrollbar.size;
			int count = _tiles.Count;
			int num = _tiles.FindIndex((ListMetaCardView_Expanding x) => x.Card.GrpId == grpId);
			if (num != -1)
			{
				float max = (float)num / ((1f - size) * (float)count);
				int num2 = ((_tiles[num].Quantity != 1) ? 1 : 2);
				float min = ((float)(num + num2) - size * (float)count) / ((1f - size) * (float)count);
				float value = 1f - Mathf.Clamp(1f - _scrollbar.value, min, max);
				_scrollbar.value = value;
			}
		}
	}

	public override void ClearCards()
	{
		foreach (ListMetaCardView_Expanding tile in _tiles)
		{
			UnityEngine.Object.Destroy(tile.gameObject);
		}
		_tiles.Clear();
	}

	public void SetCommanderCards(ListMetaCardViewDisplayInformation di)
	{
		_commanderHolder.gameObject.SetActive(value: true);
		if (di.Card != null)
		{
			CardData cardData = CardDataExtensions.CreateSkinCard(di.Card.GrpId, base.CardDatabase, di.SkinCode);
			FrameSpriteData frameSpriteData = PickFrameSpriteData(di.SkinCode, _frameSpritesData);
			Sprite frameSprite = GetFrameSprite(cardData, frameSpriteData);
			UnityEngine.Color titleTextColor = frameSpriteData.TitleTextColor;
			if (!di.Card.HasPartnerAbility() && PartnerSlotActive())
			{
				ClearCommanderSlot();
			}
			_commanderHolder.SetCard(cardData, frameSprite, titleTextColor, di, base.CardDatabase.GreLocProvider);
		}
		else
		{
			ClearCommanderSlot();
		}
	}

	public void SetPartnerCards(ListMetaCardViewDisplayInformation di)
	{
		_partnerHolder.gameObject.SetActive(value: true);
		if (di.Card != null)
		{
			CardData cardData = CardDataExtensions.CreateSkinCard(di.Card.GrpId, base.CardDatabase, di.SkinCode);
			FrameSpriteData frameSpriteData = PickFrameSpriteData(di.SkinCode, _frameSpritesData);
			Sprite frameSprite = GetFrameSprite(cardData, frameSpriteData);
			UnityEngine.Color titleTextColor = frameSpriteData.TitleTextColor;
			_partnerHolder.SetCard(cardData, frameSprite, titleTextColor, di, base.CardDatabase.GreLocProvider);
		}
		else
		{
			_partnerHolder.ClearCards();
		}
	}

	public void SetCompanionCards(ListMetaCardViewDisplayInformation di)
	{
		_companionHolder.gameObject.SetActive(value: true);
		if (di.Card != null)
		{
			CardData cardData = CardDataExtensions.CreateSkinCard(di.Card.GrpId, base.CardDatabase, di.SkinCode);
			FrameSpriteData frameSpriteData = PickFrameSpriteData(di.SkinCode, _frameSpritesData);
			Sprite frameSprite = GetFrameSprite(cardData, frameSpriteData);
			UnityEngine.Color titleTextColor = frameSpriteData.TitleTextColor;
			_companionHolder.SetCard(cardData, frameSprite, titleTextColor, di, base.CardDatabase.GreLocProvider);
		}
		else
		{
			_companionHolder.ClearCards();
		}
	}

	public void ClearCommanderCards()
	{
		_commanderHolder.ClearCards();
		_commanderHolder.gameObject.SetActive(value: false);
	}

	public void ClearPartnerCards()
	{
		_partnerHolder.ClearCards();
		_partnerHolder.gameObject.SetActive(value: false);
	}

	public void ClearCompanionCards()
	{
		_companionHolder.ClearCards();
		_companionHolder.gameObject.SetActive(value: false);
	}

	private ListMetaCardView_Expanding CreateCardView(CardData cardData, ListMetaCardViewDisplayInformation di)
	{
		ListMetaCardView_Expanding listMetaCardView_Expanding = UnityEngine.Object.Instantiate(_cardPrefab);
		Transform obj = listMetaCardView_Expanding.transform;
		obj.SetParent(_cardParent);
		obj.localScale = Vector3.one;
		obj.localRotation = Quaternion.Euler(Vector3.zero);
		Vector3 localPosition = obj.localPosition;
		localPosition.z = 0f;
		obj.localPosition = localPosition;
		listMetaCardView_Expanding.Holder = this;
		listMetaCardView_Expanding.Card = cardData;
		listMetaCardView_Expanding.OnAddClicked = (Action<MetaCardView>)Delegate.Combine(listMetaCardView_Expanding.OnAddClicked, new Action<MetaCardView>(CardView_OnAddClicked));
		listMetaCardView_Expanding.OnRemoveClicked = (Action<MetaCardView>)Delegate.Combine(listMetaCardView_Expanding.OnRemoveClicked, new Action<MetaCardView>(CardView_OnRemoveClicked));
		listMetaCardView_Expanding.TagButton.enabled = ButtonsAreInteractable && !DisableAddButtonOnly;
		listMetaCardView_Expanding.TileButton.enabled = ButtonsAreInteractable;
		listMetaCardView_Expanding.ShowUnCollectedTreatment = di.Unowned;
		listMetaCardView_Expanding.ShowBannedTreatment = di.Banned;
		listMetaCardView_Expanding.ShowInvalidTreatment = di.Invalid;
		listMetaCardView_Expanding.HangerSituation = new HangerSituation
		{
			ContextualHangers = di.ContextualHangers
		};
		listMetaCardView_Expanding.SetName(base.CardDatabase.GreLocProvider.GetLocalizedText(cardData.TitleId));
		listMetaCardView_Expanding.SetQuantity((int)di.Quantity);
		FrameSpriteData frameSpriteData = PickFrameSpriteData(di.SkinCode, _frameSpritesData);
		listMetaCardView_Expanding.SetFrameSprite(GetFrameSprite(cardData, frameSpriteData));
		listMetaCardView_Expanding.SetNameColor(frameSpriteData.TitleTextColor);
		SetManaCost(listMetaCardView_Expanding);
		Sequence s = DOTween.Sequence();
		s.Append(listMetaCardView_Expanding.LayoutElement.DOMinSize(new Vector2(listMetaCardView_Expanding.LayoutElement.minWidth, 0f), _animationData.GrowDuration).From().SetEase(_animationData.GrowEase));
		s.Append(listMetaCardView_Expanding.CanvasGroup.DOFade(0f, _animationData.FadeInDuration).From().SetEase(_animationData.FadeInEase));
		return listMetaCardView_Expanding;
	}

	public static void SetManaCost(ListMetaCardView_Expanding cardView)
	{
		CardData visualCard = cardView.VisualCard;
		string text = string.Empty;
		if (visualCard.CardTypes.Count != 1 || visualCard.CardTypes[0] != CardType.Land)
		{
			text = visualCard.OldSchoolManaText;
		}
		cardView.SetManaCost(ManaUtilities.ConvertManaSymbols(text));
	}

	private void CardView_OnAddClicked(MetaCardView cardView)
	{
		OnCardAddClicked?.Invoke(cardView);
	}

	private void CardView_OnRemoveClicked(MetaCardView cardView)
	{
		OnCardRemoveClicked?.Invoke(cardView);
	}

	public static FrameSpriteData PickFrameSpriteData(string skinCode, FrameSpriteData[] frameSpriteDatas)
	{
		if (!string.IsNullOrEmpty(skinCode))
		{
			return frameSpriteDatas[1];
		}
		return frameSpriteDatas[0];
	}

	public static Sprite GetFrameSprite(CardData cardData, FrameSpriteData frameSprites)
	{
		IReadOnlyList<CardColor> getFrameColors = cardData.GetFrameColors;
		Dictionary<CardColor, bool> dictionary = new Dictionary<CardColor, bool>();
		dictionary[CardColor.White] = getFrameColors.Contains(CardColor.White);
		dictionary[CardColor.Blue] = getFrameColors.Contains(CardColor.Blue);
		dictionary[CardColor.Black] = getFrameColors.Contains(CardColor.Black);
		dictionary[CardColor.Red] = getFrameColors.Contains(CardColor.Red);
		dictionary[CardColor.Green] = getFrameColors.Contains(CardColor.Green);
		int num = dictionary.Count((KeyValuePair<CardColor, bool> kvp) => kvp.Value);
		switch (num)
		{
		case 0:
			if (cardData.CardTypes.Contains(CardType.Artifact))
			{
				return frameSprites.Artifact;
			}
			return frameSprites.Colorless;
		case 1:
			if (dictionary[CardColor.White])
			{
				return frameSprites.White;
			}
			if (dictionary[CardColor.Blue])
			{
				return frameSprites.Blue;
			}
			if (dictionary[CardColor.Black])
			{
				return frameSprites.Black;
			}
			if (dictionary[CardColor.Red])
			{
				return frameSprites.Red;
			}
			if (dictionary[CardColor.Green])
			{
				return frameSprites.Green;
			}
			Debug.LogErrorFormat("Unable to GetFrameSprite for card grpId {0} (should not reach this line of code)", cardData.GrpId);
			break;
		}
		if (num == 2)
		{
			if (dictionary[CardColor.White] && dictionary[CardColor.Blue])
			{
				return frameSprites.WhiteBlue;
			}
			if (dictionary[CardColor.White] && dictionary[CardColor.Black])
			{
				return frameSprites.WhiteBlack;
			}
			if (dictionary[CardColor.Blue] && dictionary[CardColor.Black])
			{
				return frameSprites.BlueBlack;
			}
			if (dictionary[CardColor.Blue] && dictionary[CardColor.Red])
			{
				return frameSprites.BlueRed;
			}
			if (dictionary[CardColor.Black] && dictionary[CardColor.Red])
			{
				return frameSprites.BlackRed;
			}
			if (dictionary[CardColor.Black] && dictionary[CardColor.Green])
			{
				return frameSprites.BlackGreen;
			}
			if (dictionary[CardColor.Red] && dictionary[CardColor.Green])
			{
				return frameSprites.RedGreen;
			}
			if (dictionary[CardColor.Red] && dictionary[CardColor.White])
			{
				return frameSprites.RedWhite;
			}
			if (dictionary[CardColor.Green] && dictionary[CardColor.White])
			{
				return frameSprites.GreenWhite;
			}
			if (dictionary[CardColor.Green] && dictionary[CardColor.Blue])
			{
				return frameSprites.GreenBlue;
			}
			Debug.LogErrorFormat("Unable to GetFrameSprite for card grpId {0} (should not reach this line of code)", cardData.GrpId);
		}
		return frameSprites.Multicolor;
	}

	public override DeckBuilderPile? GetParentDeckBuilderPile()
	{
		return Pantry.Get<DeckBuilderLayoutState>().IsListViewSideboarding ? DeckBuilderPile.Sideboard : DeckBuilderPile.MainDeck;
	}

	private void ClearCommanderSlot()
	{
		_commanderHolder.ClearCards();
		ClearPartnerCards();
	}

	private bool PartnerSlotActive()
	{
		return _partnerHolder.gameObject.activeSelf;
	}
}
