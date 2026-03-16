using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Mailbox;
using Assets.Core.Meta.Utilities;
using Core.Code.PlayerInbox;
using Core.Code.Promises;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Mailbox;

public class PlayerInboxLetterDisplay : MonoBehaviour
{
	[SerializeField]
	private Localize _titleText;

	[SerializeField]
	private Localize _creationDate;

	[SerializeField]
	private Image _bannerImage;

	[SerializeField]
	private RawImage _cardArtImage;

	[SerializeField]
	private GameObject _cardArt;

	[SerializeField]
	private GameObject _bannerArt;

	[SerializeField]
	private Localize _bodyText;

	[SerializeField]
	private GameObject _buttons;

	[SerializeField]
	private CustomButton _claimButton;

	[SerializeField]
	private CustomButton _moreInfoButton;

	[SerializeField]
	private Localize _messageDeleteAlertText;

	private CardArtTextureLoader _artTextureLoader;

	private AssetTracker _assetTracker = new AssetTracker();

	private AssetLoader.AssetTracker<Sprite> _backgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PlayerInboxBackgroundImageSprite");

	private AssetLookupSystem _assetLookupSystem;

	private ClientLetterViewModel _clientLetterViewModel;

	private readonly string DEFAULT_ART_ID = "0";

	private Action _onCloseLetter;

	private Action _onCloseMailbox;

	public void SetLetterViewModel(ClientLetterViewModel clientLetter)
	{
		_clientLetterViewModel = clientLetter;
		_titleText.SetText(clientLetter.Title, null, clientLetter.FallbackTitle);
		_creationDate.SetText("PlayerInbox/DateFormat", new Dictionary<string, string>
		{
			{
				"Month",
				clientLetter.CreationDate.ToString("MM")
			},
			{
				"Day",
				clientLetter.CreationDate.ToString("dd")
			},
			{
				"Year",
				clientLetter.CreationDate.ToString("yyyy")
			}
		});
		_bodyText.SetText(clientLetter.Body, null, clientLetter.FallbackBody);
		switch (clientLetter.ArtContentType)
		{
		case ELetterArtContentType.ArtId:
			SetArtIdArt(clientLetter);
			break;
		case ELetterArtContentType.CardId:
			SetCardIdArt(clientLetter);
			break;
		case ELetterArtContentType.Unknown:
			SetDefaultArt(clientLetter);
			break;
		default:
			SetDefaultArt(clientLetter);
			break;
		}
		bool flag = !string.IsNullOrWhiteSpace(clientLetter.MoreInfoHyperlink);
		_moreInfoButton.OnClick.RemoveAllListeners();
		if (flag)
		{
			if (clientLetter.MoreInfoHyperlink.StartsWith("unitydl://") && clientLetter.MoreInfoHyperlink.Contains("autoplay=true") && !clientLetter.IsRead)
			{
				MoreInfo();
			}
			_moreInfoButton.OnClick.AddListener(MoreInfo);
		}
		_moreInfoButton.gameObject.SetActive(flag);
		_claimButton.OnClick.RemoveAllListeners();
		List<TreasureItem> attachments = clientLetter.Attachments;
		bool flag2 = attachments != null && attachments.Count > 0;
		if (flag2)
		{
			if (clientLetter.IsClaimed)
			{
				_claimButton.SetText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Mailbox/Button_Claimed"));
				_claimButton.Interactable = false;
			}
			else
			{
				_claimButton.OnClick.AddListener(ClaimAttachment);
				_claimButton.SetText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Mailbox/Button_Claim"));
				_claimButton.Interactable = true;
			}
			_claimButton.gameObject.SetActive(value: true);
		}
		else
		{
			_claimButton.gameObject.SetActive(value: false);
		}
		_buttons.SetActive(flag2 || flag);
		DateTime? deleteDate = clientLetter.DeleteDate;
		DateTime dateTime;
		if (deleteDate.HasValue)
		{
			DateTime valueOrDefault = deleteDate.GetValueOrDefault();
			if (valueOrDefault < clientLetter.ExpiryDate)
			{
				dateTime = valueOrDefault;
				goto IL_0269;
			}
		}
		dateTime = clientLetter.ExpiryDate;
		goto IL_0269;
		IL_0269:
		DateTime dateTime2 = dateTime;
		_messageDeleteAlertText.SetText("PlayerInbox/LetterWillBeDeletedWarning", new Dictionary<string, string>
		{
			{
				"Month",
				dateTime2.ToString("MM")
			},
			{
				"Day",
				dateTime2.ToString("dd")
			},
			{
				"Year",
				dateTime2.ToString("yyyy")
			}
		});
	}

	private void SetArtIdArt(ClientLetterViewModel clientLetter)
	{
		SetLetterArt(clientLetter.ArtContentReferenceId, clientLetter.Title);
	}

	private void SetCardIdArt(ClientLetterViewModel clientLetter)
	{
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		if (!uint.TryParse(clientLetter.ArtContentReferenceId, out var result))
		{
			SimpleLog.LogError("Failed to parse a letter's ArtContentReferenceId as a uint: " + clientLetter.ArtContentReferenceId);
		}
		else
		{
			SetLetterArt(cardDatabase.CardDataProvider.GetCardRecordById(result).ArtId.ToString(), clientLetter.Title);
		}
	}

	private void SetDefaultArt(ClientLetterViewModel clientLetter)
	{
		SetLetterArt(DEFAULT_ART_ID, clientLetter.Title);
	}

	private void SetLetterArt(string artId, string assetTrackingKey)
	{
		string artPath = CardArtUtil.GetArtPath(artId);
		_cardArtImage.texture = _artTextureLoader.AcquireCardArt(_assetTracker, assetTrackingKey, artPath);
		_bannerArt.SetActive(value: false);
		_cardArt.SetActive(value: true);
	}

	private void ClaimAttachment()
	{
		Pantry.Get<PlayerInboxDataProvider>().ClaimLetterAttachment(_clientLetterViewModel.Id).ThenOnMainThread(delegate(Promise<Client_LetterAttachmentClaimed> promise)
		{
			if (promise.Successful)
			{
				DisplayClaimedRewards(promise.Result.inventoryInfo.Changes);
				_claimButton.Interactable = false;
				_claimButton.SetText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Mailbox/Button_Claimed"));
			}
			else
			{
				ContentControllerPlayerInbox.DisplayClaimedRewardError(promise.Error.Message);
			}
		});
	}

	private void MoreInfo()
	{
		UrlOpener.OpenURL(_clientLetterViewModel.MoreInfoHyperlink);
	}

	public void Initialize(CardArtTextureLoader cardArtTextureLoader, AssetLookupSystem assetLookupSystem, Action onCloseLetter, Action onCloseMailbox)
	{
		_artTextureLoader = cardArtTextureLoader;
		_assetLookupSystem = assetLookupSystem;
		_onCloseLetter = onCloseLetter;
		_onCloseMailbox = onCloseMailbox;
	}

	public void CloseLetter()
	{
		_onCloseLetter?.Invoke();
	}

	public void CloseMailbox()
	{
		_onCloseMailbox?.Invoke();
	}

	public void OnEnable()
	{
		if (_clientLetterViewModel != null)
		{
			SetLetterViewModel(_clientLetterViewModel);
		}
	}

	public void OnDisable()
	{
		_claimButton.gameObject.SetActive(value: false);
		_moreInfoButton.gameObject.SetActive(value: false);
		_claimButton.OnClick.RemoveListener(ClaimAttachment);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_bannerImage, _backgroundImageSpriteTracker);
	}

	public static string GetBackgroundImagePath(AssetLookupSystem assetLookupSystem, string bannerArtId)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LetterBannerArtId = bannerArtId;
		LetterDisplayEventBGPayload payload = assetLookupSystem.TreeLoader.LoadTree<LetterDisplayEventBGPayload>().GetPayload(assetLookupSystem.Blackboard);
		assetLookupSystem.Blackboard.Clear();
		return payload?.Reference.RelativePath;
	}

	private void DisplayClaimedRewards(List<InventoryChange> changes)
	{
		SceneLoader.GetSceneLoader().GetPlayerInboxContentController().DisplayClaimedRewards(changes);
	}
}
