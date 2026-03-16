using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using GreClient.CardData;
using UnityEngine;
using Wizards.Arena.DeckValidation.Core.Models;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class BannedCardPopup : PopupBase
{
	public enum BannedType
	{
		Banned,
		Suspended,
		Restricted
	}

	private class BannedCardsViewModel
	{
		public DeckFormat Format;

		public BannedType BannedType;

		public HashSet<uint> CardTitleIdsToAcknowledge = new HashSet<uint>();
	}

	[SerializeField]
	private RectTransform _locatorBannedCard;

	[SerializeField]
	private BannedCard _bannedCardPrefab;

	[SerializeField]
	private Vector3 ScrollUpperLeft = Vector3.zero;

	[SerializeField]
	private Vector3 CardOffset = Vector3.zero;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	[Header("UI Text")]
	[SerializeField]
	private Localize _title;

	[SerializeField]
	private Localize _subtitle;

	[SerializeField]
	private Localize _subtitle2;

	[SerializeField]
	private Localize _footer;

	private ICardDatabaseAdapter _cardDatabase;

	private FormatManager _formatManager;

	private readonly List<ClientInventoryUpdateReportItem> _associatedInventoryUpdates = new List<ClientInventoryUpdateReportItem>();

	private readonly List<BannedCard> _spawnedBannedCards = new List<BannedCard>();

	private List<BannedCardsViewModel> _viewModels;

	private BannedCardsViewModel _currentViewModel;

	private Action _onCompleted;

	public bool ShouldBeShown { get; private set; }

	public override void OnEscape()
	{
		Okay();
	}

	public override void OnEnter()
	{
		Okay();
	}

	protected override void Awake()
	{
		base.Awake();
		_cardDatabase = WrapperController.Instance.CardDatabase;
		_cardHolder.RolloverZoomView = SceneLoader.GetSceneLoader().GetCardZoomView();
		_cardHolder.EnsureInit(WrapperController.Instance.CardDatabase, WrapperController.Instance.CardViewBuilder);
		_formatManager = Pantry.Get<FormatManager>();
	}

	public void Okay()
	{
		_locatorBannedCard.DestroyChildren();
		_spawnedBannedCards.Clear();
		BannedCardsViewModel bannedCardsViewModel = NextViewModel(_viewModels);
		if (bannedCardsViewModel != null)
		{
			ShowCards(bannedCardsViewModel);
			return;
		}
		if (_associatedInventoryUpdates.Any())
		{
			ContentControllerRewards rewardsContentController = SceneLoader.GetSceneLoader().GetRewardsContentController();
			rewardsContentController.RegisterRewardWillCloseCallback(CleanUp);
			rewardsContentController.AddAndDisplayRewardsCoroutine(_associatedInventoryUpdates, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Vault/DraftProgressOkay"));
			_associatedInventoryUpdates.Clear();
		}
		else
		{
			CleanUp();
		}
		ShouldBeShown = false;
		Hide();
	}

	private void CleanUp()
	{
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.BannedCardGrant, OnInventoryUpdated);
		WrapperController.Instance.InventoryManager.UnSubscribe(InventoryUpdateSource.RestrictedCardGrant, OnInventoryUpdated);
		_onCompleted();
	}

	private void OnInventoryUpdated(ClientInventoryUpdateReportItem update)
	{
		_associatedInventoryUpdates.Add(update);
	}

	private static string GetLocKeyForBannedCards(string root, string textArea, BannedType bannedType, string formatName)
	{
		string text = bannedType.ToString();
		return root + "/" + textArea + "_" + text + "_" + formatName;
	}

	private void SetUIForState(string formatName, BannedType bannedType)
	{
		_title.SetText(GetLocKeyForBannedCards("MainNav/BannedCards", "Title", bannedType, formatName));
		_subtitle.SetText(GetLocKeyForBannedCards("MainNav/BannedCards", "Subtitle", bannedType, formatName));
		_subtitle2.SetText(GetLocKeyForBannedCards("MainNav/BannedCards", "Playability", bannedType, formatName));
		_footer.SetText(GetLocKeyForBannedCards("MainNav/BannedCards", "WildcardReplacement", bannedType, formatName));
	}

	public void Setup(Action onComplete)
	{
		_viewModels = new List<BannedCardsViewModel>();
		_onCompleted = onComplete;
		List<BannedType> list = new List<BannedType>
		{
			BannedType.Banned,
			BannedType.Suspended,
			BannedType.Restricted
		};
		foreach (DeckFormat bannedPopupFormat in _formatManager.BannedPopupFormats)
		{
			foreach (BannedType item2 in list)
			{
				HashSet<uint> hashSet = CardsIdsToShow(bannedPopupFormat, item2);
				if (hashSet.Count > 0)
				{
					BannedCardsViewModel item = new BannedCardsViewModel
					{
						Format = bannedPopupFormat,
						BannedType = item2,
						CardTitleIdsToAcknowledge = hashSet
					};
					_viewModels.Add(item);
				}
			}
		}
		ShouldBeShown = _viewModels.Count > 0;
	}

	private HashSet<uint> CardsIdsToShow(DeckFormat format, BannedType bannedType)
	{
		HashSet<uint> hashSet = new HashSet<uint>();
		AccountInformation accountInformation = _accountClient.AccountInformation;
		if (accountInformation != null)
		{
			switch (bannedType)
			{
			case BannedType.Suspended:
				if (format.SuspendedCardTitleIds != null)
				{
					hashSet.UnionWith(format.SuspendedCardTitleIds);
				}
				break;
			case BannedType.Banned:
				if (format.BannedTitleIds != null && format.SuspendedCardTitleIds != null)
				{
					hashSet.UnionWith(format.BannedTitleIds);
					hashSet.ExceptWith(format.SuspendedCardTitleIds);
				}
				else if (format.BannedTitleIds != null)
				{
					hashSet.UnionWith(format.BannedTitleIds);
				}
				break;
			case BannedType.Restricted:
			{
				Dictionary<uint, Quota> restrictedTitleIds = format.RestrictedTitleIds;
				if (restrictedTitleIds != null && restrictedTitleIds.Count > 0)
				{
					hashSet.UnionWith(format.RestrictedTitleIds.Select((KeyValuePair<uint, Quota> x) => x.Key));
				}
				break;
			}
			}
			if (format.SupressedCardTitleIds != null)
			{
				hashSet.ExceptWith(format.SupressedCardTitleIds);
			}
			string personaID = accountInformation.PersonaID;
			string text = bannedType switch
			{
				BannedType.Suspended => MDNPlayerPrefs.GetAcknowledgedSuspendedCardsForFormat(personaID, format.FormatName), 
				BannedType.Restricted => MDNPlayerPrefs.GetAcknowledgedRestrictedCardsForFormat(personaID, format.FormatName), 
				BannedType.Banned => MDNPlayerPrefs.GetAcknowledgedBannedCardsForFormat(personaID, format.FormatName), 
				_ => "", 
			};
			if (!string.IsNullOrWhiteSpace(text) && !WrapperController.Instance.DebugFlag.BannedPopup)
			{
				string[] array = text.Split('|');
				foreach (string text2 in array)
				{
					if (uint.TryParse(text2, out var result))
					{
						hashSet.Remove(result);
						continue;
					}
					IReadOnlyList<CardPrintingData> printingsByEnglishTitle = _cardDatabase.DatabaseUtilities.GetPrintingsByEnglishTitle(text2);
					if (printingsByEnglishTitle != null && printingsByEnglishTitle.Count > 0)
					{
						hashSet.Remove(printingsByEnglishTitle[0].TitleId);
					}
				}
			}
		}
		return hashSet;
	}

	private void ShowCards(BannedCardsViewModel viewModel)
	{
		SetUIForState(viewModel.Format.FormatName, viewModel.BannedType);
		UpdateAcknowledgedCards(viewModel.Format, viewModel.CardTitleIdsToAcknowledge, viewModel.BannedType);
	}

	private static BannedCardsViewModel NextViewModel(List<BannedCardsViewModel> viewModels)
	{
		if (viewModels.Count > 0)
		{
			BannedCardsViewModel result = viewModels[0];
			viewModels.RemoveAt(0);
			return result;
		}
		return null;
	}

	protected override void Show()
	{
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.BannedCardGrant, OnInventoryUpdated);
		WrapperController.Instance.InventoryManager.Subscribe(InventoryUpdateSource.RestrictedCardGrant, OnInventoryUpdated);
		BannedCardsViewModel viewModel = NextViewModel(_viewModels);
		ShowCards(viewModel);
		base.Show();
	}

	private void UpdateAcknowledgedCards(DeckFormat format, IEnumerable<uint> cardTitleIdsToAcknowledge, BannedType bannedType = BannedType.Banned)
	{
		foreach (uint item in cardTitleIdsToAcknowledge)
		{
			BannedCard bannedCard = UnityEngine.Object.Instantiate(_bannedCardPrefab, _locatorBannedCard);
			_spawnedBannedCards.Add(bannedCard);
			bannedCard.ShowBannedCard(item, _cardHolder);
		}
		StartCoroutine(DelayedLayoutCards());
		AccountInformation accountInformation = _accountClient.AccountInformation;
		if (accountInformation == null)
		{
			return;
		}
		string personaID = accountInformation.PersonaID;
		switch (bannedType)
		{
		case BannedType.Suspended:
			MDNPlayerPrefs.SetAcknowledgedSuspendedCardsForFormat(personaID, format.FormatName, string.Join("|", format.SuspendedCardTitleIds));
			return;
		case BannedType.Restricted:
			if (format.RestrictedTitleIds != null && format.RestrictedTitleIds.Count > 0)
			{
				MDNPlayerPrefs.SetAcknowledgedRestrictedCardsForFormat(personaID, format.FormatName, string.Join("|", format.RestrictedTitleIds.Select((KeyValuePair<uint, Quota> x) => x.Key)));
				return;
			}
			break;
		}
		string collatedCardNames = string.Join("|", format.BannedTitleIds.Union(format.SupressedCardTitleIds));
		MDNPlayerPrefs.SetAcknowledgedBannedCardsForFormat(personaID, format.FormatName, collatedCardNames);
	}

	private IEnumerator DelayedLayoutCards()
	{
		yield return null;
		LayoutCards();
	}

	private void LayoutCards()
	{
		int count = _spawnedBannedCards.Count;
		_locatorBannedCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardOffset.x * (float)(count - 1) + ScrollUpperLeft.x * 2f);
		_locatorBannedCard.anchoredPosition = _locatorBannedCard.sizeDelta;
		for (int i = 0; i < _spawnedBannedCards.Count; i++)
		{
			int num = i % count;
			Vector3 scrollUpperLeft = ScrollUpperLeft;
			scrollUpperLeft.x += CardOffset.x * (float)num;
			BannedCard bannedCard = _spawnedBannedCards[i];
			RectTransform component = bannedCard.GetComponent<RectTransform>();
			if ((bool)component)
			{
				component.anchoredPosition3D = scrollUpperLeft;
			}
			else
			{
				bannedCard.transform.localPosition = scrollUpperLeft;
			}
		}
	}
}
