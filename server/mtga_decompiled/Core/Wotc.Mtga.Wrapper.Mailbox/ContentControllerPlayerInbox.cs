using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Input;
using Core.Code.PlayerInbox;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Wrapper.Mailbox;

public class ContentControllerPlayerInbox : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	private PlayerInboxDataProvider _playerInboxDataProvider;

	private List<Client_Letter> _cachedLetters;

	private List<Client_Letter> _lettersToUpdate;

	private PlayerInboxBladeItemDisplay _selectedBladeItem;

	private GameObject _noMessagesBladeInstance;

	private Dictionary<Guid, PlayerInboxBladeItemDisplay> _letterListItems;

	private CardArtTextureLoader _cardArtTextureLoader;

	private AssetLookupSystem _assetLookupSystem;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	[SerializeField]
	private CustomButton _dismissButton;

	[SerializeField]
	private GameObject _currentlyDisplayedLetter;

	[SerializeField]
	private GameObject _letterBladePrefab;

	[SerializeField]
	private GameObject _noMessagesBladePrefab;

	[SerializeField]
	private GameObject _emptyLetter;

	public Transform BladeItemTransform;

	public Transform LetterTransform;

	private IBILogger _biLogger;

	private PlayerInboxBladeItemDisplay SelectedBladeItem
	{
		get
		{
			return _selectedBladeItem;
		}
		set
		{
			if (_selectedBladeItem != null && _selectedBladeItem != value)
			{
				_selectedBladeItem.DeselectLetter();
			}
			_selectedBladeItem = value;
		}
	}

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	public void Init(PlayerInboxDataProvider playerInboxDataProvider, CardArtTextureLoader cardArtTextureLoader, AssetLookupSystem assetLookupSystem, IBILogger biLogger)
	{
		_keyboardManager = Pantry.Get<KeyboardManager>();
		_actionSystem = Pantry.Get<IActionSystem>();
		_playerInboxDataProvider = playerInboxDataProvider;
		_cardArtTextureLoader = cardArtTextureLoader;
		_assetLookupSystem = assetLookupSystem;
		_letterListItems = new Dictionary<Guid, PlayerInboxBladeItemDisplay>();
		_currentlyDisplayedLetter = UnityEngine.Object.Instantiate(_currentlyDisplayedLetter, LetterTransform);
		_biLogger = biLogger;
	}

	private void Awake()
	{
		_dismissButton.OnClick.AddListener(Hide);
	}

	private void OnDestroy()
	{
		_dismissButton.OnClick.RemoveListener(Hide);
		_playerInboxDataProvider.UnRegisterForLetterChanges(OnLettersChanged);
		_letterBladePrefab = null;
		_currentlyDisplayedLetter = null;
		_keyboardManager?.Unsubscribe(this);
	}

	private void OnLettersChanged(PlayerInboxDataProvider.LetterDataChange changeType, List<Client_Letter> letters)
	{
		switch (changeType)
		{
		case PlayerInboxDataProvider.LetterDataChange.Partial:
			if (_lettersToUpdate == null)
			{
				_lettersToUpdate = new List<Client_Letter>();
			}
			_lettersToUpdate.AddRange(letters);
			break;
		case PlayerInboxDataProvider.LetterDataChange.All:
			_cachedLetters = letters;
			break;
		default:
			throw new ArgumentOutOfRangeException("changeType", changeType, null);
		case PlayerInboxDataProvider.LetterDataChange.Error:
			break;
		}
	}

	public void Update()
	{
		if (_cachedLetters != null)
		{
			DisplayLetters(_cachedLetters);
		}
		else if (_lettersToUpdate != null)
		{
			foreach (Client_Letter item in _lettersToUpdate)
			{
				UpdateLetterDisplayItem(item);
			}
		}
		_lettersToUpdate = null;
		_cachedLetters = null;
	}

	public void Show()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (sceneLoader != null && sceneLoader.CurrentContentType == NavContentType.Store)
		{
			sceneLoader.DisableBonusPackProgressMeter();
		}
		_playerInboxDataProvider.GetPlayerInbox().Then(delegate(Promise<List<Client_Letter>> p)
		{
			if (p.Successful)
			{
				_cachedLetters = p.Result;
			}
		}).Then(delegate
		{
			_playerInboxDataProvider.RegisterForLetterChanges(OnLettersChanged);
		});
		base.gameObject.UpdateActive(active: true);
		_keyboardManager?.Subscribe(this);
		_actionSystem?.PushFocus(this);
	}

	public void Hide()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (base.gameObject.activeInHierarchy && sceneLoader != null && sceneLoader.CurrentContentType == NavContentType.Store)
		{
			sceneLoader.EnableBonusPackProgressMeter();
		}
		CloseCurrentLetter();
		_playerInboxDataProvider.UnRegisterForLetterChanges(OnLettersChanged);
		base.gameObject.UpdateActive(active: false);
		_keyboardManager?.Unsubscribe(this);
		_actionSystem?.PopFocus(this);
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			Hide();
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		Hide();
	}

	private void UpdateNoMessageDisplay(bool hasMessages)
	{
		if (hasMessages)
		{
			if (_noMessagesBladeInstance != null)
			{
				UnityEngine.Object.Destroy(_noMessagesBladeInstance);
				_noMessagesBladeInstance = null;
			}
		}
		else if (_noMessagesBladeInstance == null)
		{
			_noMessagesBladeInstance = UnityEngine.Object.Instantiate(_noMessagesBladePrefab, BladeItemTransform);
		}
	}

	public void DisplayLetters(List<Client_Letter> lettersToDisplay)
	{
		ClearLettersDisplay();
		if (!lettersToDisplay.Any())
		{
			UpdateNoMessageDisplay(hasMessages: false);
			return;
		}
		UpdateNoMessageDisplay(hasMessages: true);
		if (_noMessagesBladeInstance != null)
		{
			UnityEngine.Object.Destroy(_noMessagesBladeInstance);
			_noMessagesBladeInstance = null;
		}
		foreach (Client_Letter item in lettersToDisplay.OrderByDescending((Client_Letter letter) => letter.CreatedDate))
		{
			GameObject obj = UnityEngine.Object.Instantiate(_letterBladePrefab, BladeItemTransform);
			PlayerInboxBladeItemDisplay component = obj.GetComponent<PlayerInboxBladeItemDisplay>();
			component.Initialize(OnLetterSelected);
			component.SetLetterViewModel(ConvertClientLetterToViewModel(item));
			obj.SetActive(value: true);
			_letterListItems[item.Id] = component;
		}
	}

	private void ClearLettersDisplay()
	{
		SelectedBladeItem = null;
		foreach (KeyValuePair<Guid, PlayerInboxBladeItemDisplay> letterListItem in _letterListItems)
		{
			letterListItem.Deconstruct(out var _, out var value);
			UnityEngine.Object.Destroy(value.gameObject);
		}
		_letterListItems.Clear();
	}

	private void UpdateLetterDisplayItem(Client_Letter letter)
	{
		if (_letterListItems.TryGetValue(letter.Id, out var value))
		{
			value.SetLetterViewModel(ConvertClientLetterToViewModel(letter));
			if (letter.Id == SelectedBladeItem._clientBladeItemViewModel.Id)
			{
				_currentlyDisplayedLetter.GetComponent<PlayerInboxLetterDisplay>().SetLetterViewModel(value._clientBladeItemViewModel);
				value.SetSelectedView();
			}
		}
	}

	private void OnLetterSelected(PlayerInboxBladeItemDisplay selectedLetter, bool isRead, Guid selectedLetterId)
	{
		if (!(SelectedBladeItem?._clientBladeItemViewModel?.Id == selectedLetter?._clientBladeItemViewModel?.Id))
		{
			CloseCurrentLetter();
			OpenSelectedLetter(selectedLetter);
			if (!isRead)
			{
				_playerInboxDataProvider.MarkLetterRead(selectedLetterId);
			}
		}
	}

	private void OpenSelectedLetter(PlayerInboxBladeItemDisplay selectedLetter)
	{
		SelectedBladeItem = selectedLetter;
		PlayerInboxLetterDisplay component = _currentlyDisplayedLetter.GetComponent<PlayerInboxLetterDisplay>();
		component.Initialize(_cardArtTextureLoader, _assetLookupSystem, CloseCurrentLetter, Hide);
		component.SetLetterViewModel(selectedLetter._clientBladeItemViewModel);
		_currentlyDisplayedLetter.SetActive(value: true);
		_emptyLetter.UpdateActive(active: false);
		InboxLetterOpened payload = new InboxLetterOpened
		{
			EventTime = DateTime.UtcNow,
			letterType = selectedLetter._clientBladeItemViewModel.LetterType,
			messageId = selectedLetter._clientBladeItemViewModel.Id
		};
		_biLogger.Send(ClientBusinessEventType.InboxLetterOpened, payload);
	}

	public void CloseCurrentTopLevel()
	{
		if (SelectedBladeItem == null)
		{
			Hide();
		}
		else
		{
			CloseCurrentLetter();
		}
	}

	private void CloseCurrentLetter()
	{
		SelectedBladeItem = null;
		_currentlyDisplayedLetter.SetActive(value: false);
		_emptyLetter.UpdateActive(active: true);
	}

	public ClientLetterViewModel ConvertClientLetterToViewModel(Client_Letter clientLetter)
	{
		return new ClientLetterViewModel
		{
			Id = clientLetter.Id,
			Title = clientLetter.Content.Title,
			FallbackTitle = clientLetter.Content.FallbackTitle,
			Body = clientLetter.Content.Body,
			FallbackBody = clientLetter.Content.FallbackBody,
			Attachments = clientLetter.Content.Attachments,
			ArtContentType = clientLetter.Content.ArtContentType,
			ArtContentReferenceId = clientLetter.Content.ArtContentReferenceId,
			CreationDate = clientLetter.CreatedDate.ToLocalTime(),
			ExpiryDate = clientLetter.ExpiryDate.ToLocalTime(),
			IsRead = clientLetter.State.ReadDate.HasValue,
			IsClaimed = clientLetter.State.ClaimDate.HasValue,
			DeleteDate = clientLetter.State.DeleteDate,
			LetterType = clientLetter.LetterType,
			MoreInfoHyperlink = clientLetter.Content.MoreInfoHyperlink
		};
	}

	public void DisplayClaimedRewards(List<InventoryChange> inventoryChanges, string title = "MainNav/Rewards/Rewards_Title")
	{
		List<ClientInventoryUpdateReportItem> ts = new List<ClientInventoryUpdateReportItem>();
		foreach (InventoryChange inventoryChange in inventoryChanges)
		{
			ts.Add(inventoryChange.ToUpdateReportItem());
		}
		SceneLoader.GetSceneLoader().GetRewardsContentController().AddAndDisplayRewardsCoroutine(ts, Languages.ActiveLocProvider.GetLocalizedText(title), "MainNav/EventRewards/Claim_Prizes");
	}

	public static void DisplayClaimedRewardError(string errorMessage)
	{
		PromiseExtensions.Logger.Error(errorMessage);
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Mailbox/Claim_Error"));
	}
}
