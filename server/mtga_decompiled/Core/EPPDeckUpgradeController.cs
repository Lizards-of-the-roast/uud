using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Promises;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class EPPDeckUpgradeController : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	[SerializeField]
	private float _cardRevealDelay = 0.1f;

	[SerializeField]
	private float _bundleAddDelay = 0.25f;

	[SerializeField]
	private float _cardInsertDelay = 0.1f;

	[SerializeField]
	private CardBundle _cardBundlePrefab;

	[SerializeField]
	private Transform _cardContainer;

	[SerializeField]
	private Localize _titleLabel;

	[SerializeField]
	private Localize _subtitleLabel;

	[SerializeField]
	private Animator _deckBoxAnimator;

	[SerializeField]
	private MeshRenderer[] _deckBoxMaterials;

	[SerializeField]
	private CustomButton _upgradeDeckButton;

	[SerializeField]
	private CustomButton _dismissDeckButton;

	[SerializeField]
	private Localize _clickToContinue;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	private UpgradePacket _upgrade;

	private Client_Deck _deck;

	private bool _testing;

	private MeshRendererReferenceLoader[] _meshRendererReferenceLoaders;

	public void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_cardHolder.RolloverZoomView = SceneLoader.GetSceneLoader().GetCardZoomView();
		_cardHolder.ShowHighlight = (MetaCardView c) => false;
		_upgradeDeckButton.OnClick.AddListener(OnUpgradeButtonPressed);
		_dismissDeckButton.OnClick.AddListener(OnDismissButtonPressed);
		_meshRendererReferenceLoaders = new MeshRendererReferenceLoader[_deckBoxMaterials.Length];
		for (int num = 0; num < _deckBoxMaterials.Length; num++)
		{
			_meshRendererReferenceLoaders[num] = new MeshRendererReferenceLoader(_deckBoxMaterials[num]);
		}
	}

	private void OnDestroy()
	{
		if (_meshRendererReferenceLoaders != null)
		{
			MeshRendererReferenceLoader[] meshRendererReferenceLoaders = _meshRendererReferenceLoaders;
			for (int i = 0; i < meshRendererReferenceLoaders.Length; i++)
			{
				meshRendererReferenceLoaders[i]?.Cleanup();
			}
			_meshRendererReferenceLoaders = null;
		}
	}

	public void OnDisable()
	{
		_upgrade = null;
		foreach (Transform item in _cardContainer)
		{
			item.gameObject.UpdateActive(active: false);
		}
	}

	public void DisplayUpgrade(UpgradePacket upgrade, MTGALocalizedString title, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, bool testing = false)
	{
		_upgrade = upgrade;
		_testing = testing;
		if (_upgrade != null)
		{
			base.gameObject.SetActive(value: true);
			StartCoroutine(Coroutine_DisplayUpgrade(title, cardDatabase, cardViewBuilder, cardMaterialBuilder));
		}
	}

	private IEnumerator Coroutine_DisplayUpgrade(MTGALocalizedString title, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder)
	{
		DecksManager dm = WrapperController.Instance.DecksManager;
		_titleLabel.SetText(title);
		yield return dm.GetAllDecks().AsCoroutine();
		_deck = dm.GetDeckFromDescription(_upgrade.targetDeckDescription);
		if (_deck != null)
		{
			_subtitleLabel.SetText("EPP/DeckUpgrade/Explanation", new Dictionary<string, string> { 
			{
				"deckInfo",
				_deck.Summary.Name
			} });
			_subtitleLabel.gameObject.UpdateActive(active: true);
			_deckBoxAnimator.gameObject.UpdateActive(active: true);
			_clickToContinue.gameObject.UpdateActive(active: false);
			string artPath = ((_deck.Summary.DeckArtId == 0) ? (cardDatabase.CardDataProvider.GetCardPrintingById(_deck.Summary.DeckTileId)?.ImageAssetPath ?? string.Empty) : cardDatabase.DatabaseUtilities.GetPrintingsByArtId(_deck.Summary.DeckArtId)?.FirstOrDefault()?.ImageAssetPath);
			DeckBoxUtil.SetDeckBoxTexture(artPath, cardMaterialBuilder.TextureLoader, cardMaterialBuilder.CropDatabase, _meshRendererReferenceLoaders);
		}
		else
		{
			_subtitleLabel.gameObject.UpdateActive(active: false);
			_deckBoxAnimator.gameObject.UpdateActive(active: false);
			_clickToContinue.gameObject.UpdateActive(active: true);
		}
		List<CardPrintingData> list = (from id in _upgrade.cardsAdded
			select cardDatabase.CardDataProvider.GetCardPrintingById(id) into c
			orderby c.Rarity, c.GrpId
			select c).ToList();
		int childIndex = 0;
		uint cardGrpId = 0u;
		CardBundle currentBundle = null;
		foreach (CardPrintingData card in list)
		{
			if (cardGrpId != card.GrpId)
			{
				yield return new WaitForSeconds(_bundleAddDelay);
				cardGrpId = card.GrpId;
				if (childIndex < _cardContainer.childCount)
				{
					currentBundle = _cardContainer.GetChild(childIndex).GetComponent<CardBundle>();
					currentBundle.gameObject.UpdateActive(active: true);
				}
				else
				{
					currentBundle = Object.Instantiate(_cardBundlePrefab, _cardContainer);
				}
				childIndex++;
				currentBundle.Init(new CardData(null, card), cardDatabase, cardViewBuilder);
			}
			currentBundle.RevealCard();
			yield return new WaitForSeconds(_cardRevealDelay);
		}
	}

	public void OnUpgradeButtonPressed()
	{
		if (_deck != null)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			StartCoroutine(Coroutine_UpgradeDeck(_deck, _upgrade));
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			OnDismissButtonPressed();
		}
	}

	public void OnDismissButtonPressed()
	{
		GetComponent<Animator>().SetTrigger("Outro");
	}

	private IEnumerator Coroutine_UpgradeDeck(Client_Deck deck, UpgradePacket upgrade)
	{
		DeckBuilderModel deckBuilderModel = new DeckBuilderModel(WrapperController.Instance.CardDatabase, DeckServiceWrapperHelpers.ToAzureModel(deck), WrapperController.Instance.InventoryManager.Cards.ToDictionary((KeyValuePair<uint, int> x) => x.Key, (KeyValuePair<uint, int> x) => (uint)x.Value), isConstructed: true, isSideboarding: false, (uint)WrapperController.Instance.FormatManager.GetDefaultFormat().MaxCardsByTitle);
		uint[] array = EPPCardHitList.CardRemovalByDeck[deck.Summary.Description];
		int num = 0;
		uint num2 = 0u;
		foreach (uint item in upgrade.cardsAdded)
		{
			CardPrintingData cardPrintingById = WrapperController.Instance.CardDatabase.CardDataProvider.GetCardPrintingById(item);
			if (deckBuilderModel.GetQuantityInMainDeckByTitle(cardPrintingById) >= 4)
			{
				continue;
			}
			deckBuilderModel.AddCardToMainDeck(item);
			bool flag = false;
			for (; num < array.Length; num++)
			{
				num2 = array[num];
				if (deckBuilderModel.GetQuantityInMainDeck(num2) != 0)
				{
					deckBuilderModel.RemoveCardFromMainDeck(num2);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			CardPrintingData cardPrintingById2 = WrapperController.Instance.CardDatabase.CardDataProvider.GetCardPrintingById(num2);
			bool flag2 = cardPrintingById2.Types.Contains(CardType.Creature);
			uint convertedManaCost = cardPrintingById2.ConvertedManaCost;
			CardRarity rarity = cardPrintingById2.Rarity;
			int num3 = int.MaxValue;
			uint grpId = 0u;
			foreach (CardPrintingQuantity item2 in deckBuilderModel.GetFilteredMainDeck())
			{
				if (deckBuilderModel.GetQuantityInMainDeck(item2.Printing.GrpId) != 0 && item2.Quantity != 0)
				{
					int num4 = item2.Printing.Rarity - rarity;
					uint convertedManaCost2 = item2.Printing.ConvertedManaCost;
					uint num5 = ((convertedManaCost2 > convertedManaCost) ? (convertedManaCost2 - convertedManaCost) : (convertedManaCost - convertedManaCost2));
					int num6 = num4 + (int)num5;
					if (item2.Printing.Types.Contains(CardType.Creature) != flag2)
					{
						num6 += 2;
					}
					if (num6 < num3)
					{
						num3 = num6;
						grpId = item2.Printing.GrpId;
					}
				}
			}
			deckBuilderModel.RemoveCardFromMainDeck(grpId);
		}
		if (!_testing)
		{
			Client_Deck deck2 = DeckServiceWrapperHelpers.ToClientModel(deckBuilderModel.GetServerModel());
			Promise<Client_DeckSummary> updateRequest = WrapperController.Instance.DecksManager.UpdateDeck(deck2, DeckActionType.Updated);
			yield return updateRequest.AsCoroutine();
			if (!updateRequest.Successful)
			{
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
				yield break;
			}
		}
		_deckBoxAnimator.SetTrigger("Open");
		yield return null;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, base.gameObject);
		while (_deckBoxAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
		{
			yield return null;
		}
		CardBundleCardAnimation finalCard = null;
		foreach (Transform item3 in _cardContainer)
		{
			if (item3.gameObject.activeSelf)
			{
				CardBundle bundle = item3.GetComponent<CardBundle>();
				for (int i = bundle.TotalCards - 1; i >= 0; i--)
				{
					finalCard = bundle.InsertCard(i, _deckBoxAnimator.transform);
					finalCard.WaitForComplete = true;
					yield return new WaitForSeconds(_cardInsertDelay);
				}
			}
		}
		yield return new WaitUntil(() => !finalCard.WaitForComplete);
		_deckBoxAnimator.SetTrigger("Outro");
		yield return null;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_close, base.gameObject, 0.25f);
		while (_deckBoxAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		OnDismissButtonPressed();
	}

	public void Animation_InsertDeck(CardBundleCardAnimation card)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_return_card, base.gameObject);
		_deckBoxAnimator.SetTrigger("Insert");
		card.WaitForComplete = false;
		card.gameObject.SetActive(value: false);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_clickToContinue.isActiveAndEnabled)
		{
			OnDismissButtonPressed();
		}
	}
}
