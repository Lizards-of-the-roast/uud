using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Meta.MainNavigation.Cosmetics;
using UnityEngine;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

public class EmoteSelectionController : ICosmeticSelector<List<string>>
{
	public enum EmoteUISection
	{
		None,
		Classic,
		Sticker,
		Expansion
	}

	private const string STICKERS_LOC_KEY = "MainNav/Profile/Emotes/EquippedStickersCount_Header";

	private const string PHRASES_LOC_KEY = "MainNav/Profile/Emotes/EquippedPhrasesCount_Header";

	private const string UNLOCK_NEW_EMOTES_LOC_KEY = "MainNav/Profile/Emotes/UnlockNewEmotes";

	private const string EXCEEDED_MAX_EQUIPPED_EMOTES_LOC_KEY = "MainNav/Profile/Emotes/ExceededMaxEquippedEmotes_Header";

	private const string MAX_EQUIPPED_EMOTE_CAP_REACHED_HEADER_LOC_KEY = "MainNav/Profile/Emotes/MaxEquippedEmoteCapReached_Header";

	private const string MAX_EQUIPPED_EMOTE_CAP_REACHED_BODY_LOC_KEY = "MainNav/Profile/Emotes/MaxEquippedEmoteCapReached_Body";

	private const string UNSAVED_CHANGES_HEADER_LOC_KEY = "MainNav/DeckBuilder/DeckBuilder_MessageTitle_ConfirmSavingDeck";

	private const string UNSAVED_CHANGES_BODY_LOC_KEY = "MainNav/Profile/Emotes/UnsavedChanges_Body";

	private const string CANCEL_BUTTON_LOC_KEY = "MainNav/Popups/Modal/ModalOptions_Cancel";

	private const string DISCARD_CHANGES_BUTTON_LOC_KEY = "MainNav/DeckBuilder/DiscardChanges_Button";

	private const string SAVE_AND_EXIT_BUTTON_LOC_KEY = "MainNav/DeckBuilder/SaveAndExit_Button";

	private readonly CosmeticsProvider _cosmetics;

	private readonly IEmoteDataProvider _emoteDataProvider;

	private readonly EmoteSelectionScreenView _emoteSelectionPopupView;

	private readonly IBILogger _biLogger;

	private AssetLookupSystem _assetLookupSystem;

	private AssetLookupTree<EmoteViewPrefab> _emoteViewPrefabTree;

	private readonly Dictionary<string, EmoteView> _instantiatedEmotes = new Dictionary<string, EmoteView>();

	private string _visibleSampleEmoteId = "";

	private List<EmoteView> _sampleEmotes = new List<EmoteView>();

	private HashSet<string> _equippedEmotesOnShow;

	private HashSet<string> _equippedEmotesOnHide;

	private int _equippedStickersCount;

	private int _equippedPhrasesCount;

	private IClientLocProvider _localization;

	private Action OnHide;

	private Action<List<string>> OnSelected;

	public bool HasUnsavedChanges { get; private set; }

	public EmoteSelectionController(CosmeticsProvider cosmetics, IEmoteDataProvider emoteDataProvider, AssetLookupSystem assetLookupSystem, EmoteSelectionScreenView emoteSelectionPopupView, IBILogger biLogger, IClientLocProvider localization)
	{
		_localization = localization;
		_cosmetics = cosmetics;
		_emoteDataProvider = emoteDataProvider;
		_emoteSelectionPopupView = emoteSelectionPopupView;
		_assetLookupSystem = assetLookupSystem;
		_assetLookupSystem = assetLookupSystem;
		_biLogger = biLogger;
		_emoteSelectionPopupView.OnConfirmCallback += SaveAndHide;
		_emoteSelectionPopupView.OnCloseCallback += Hide;
		_emoteViewPrefabTree = _assetLookupSystem.TreeLoader.LoadTree<EmoteViewPrefab>(returnNewTree: false);
		_instantiatedEmotes = emoteSelectionPopupView.GetPreInstantiatedEmoteViews();
		emoteSelectionPopupView.SetHoverOnBakedEmoteViews(_onEmoteViewHover);
	}

	public void SetData(List<string> selectedEmotes)
	{
		_equippedEmotesOnShow = selectedEmotes.ToHashSet();
	}

	public void Open()
	{
		_equippedEmotesOnHide = new HashSet<string>(_equippedEmotesOnShow);
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		List<EmoteView> list = new List<EmoteView>(5);
		List<EmoteView> list2 = new List<EmoteView>();
		List<EmoteView> list3 = new List<EmoteView>();
		List<EmoteViewGameObjectData> emoteViewGameObjectDataForEmoteSelectionScreen = EmoteUtils.GetEmoteViewGameObjectDataForEmoteSelectionScreen(_cosmetics, _emoteDataProvider, _equippedEmotesOnShow, _instantiatedEmotes.Keys);
		for (int num = emoteViewGameObjectDataForEmoteSelectionScreen.Count - 1; num >= 0; num--)
		{
			EmoteViewGameObjectData emoteViewGameObjectData = emoteViewGameObjectDataForEmoteSelectionScreen[num];
			if (!emoteViewGameObjectData.EmoteData.Entry.IsDefault)
			{
				EmoteView emoteView;
				if (!emoteViewGameObjectData.IsInstantiated)
				{
					emoteView = _instantiateEmote(emoteViewGameObjectData.EmoteData);
					emoteView.SetClickable(emoteViewGameObjectData.IsClickable);
					switch (emoteViewGameObjectData.EmoteUISection)
					{
					case EmoteUISection.Classic:
						list.Add(emoteView);
						break;
					case EmoteUISection.Sticker:
						list2.Add(emoteView);
						break;
					default:
						list3.Add(emoteView);
						break;
					}
				}
				else
				{
					emoteView = _instantiatedEmotes[emoteViewGameObjectData.EmoteData.Id];
				}
				emoteView.SetEquipped(emoteViewGameObjectData.IsEquipped);
			}
		}
		_emoteSelectionPopupView.UpdateOwnedEmotes(EmoteUISection.Sticker, list2);
		_emoteSelectionPopupView.UpdateOwnedEmotes(EmoteUISection.Expansion, list3);
		_equippedPhrasesCount = _equippedEmotesOnShow.ToList().FilterByPage(_emoteDataProvider, EmotePage.Phrase).FilterByDefault(_emoteDataProvider, filterOutNonDefault: false)
			.Count;
		_equippedStickersCount = _equippedEmotesOnShow.ToList().FilterByPage(_emoteDataProvider, EmotePage.Sticker).FilterByDefault(_emoteDataProvider, filterOutNonDefault: false)
			.Count;
		_updateEmotesText(_equippedPhrasesCount, _equippedStickersCount);
		_resetNotificationText();
		_emoteSelectionPopupView.SetConfirmButtonInteractable(isInteractable: true);
		_emoteSelectionPopupView.gameObject.SetActive(value: true);
	}

	public void Close()
	{
		_emoteSelectionPopupView.gameObject.SetActive(value: false);
	}

	public void Hide()
	{
		if (HasUnsavedChanges)
		{
			ShowUnsavedChangesSystemMessage(null, _cleanUpAndHide, SaveAndHide);
		}
		else
		{
			_cleanUpAndHide();
		}
		OnHide?.Invoke();
	}

	public bool Save(Action onSaveComplete = null)
	{
		bool flag = false;
		IEmoteDataProvider emoteDataProvider = _emoteDataProvider;
		ICollection<string> equippedEmotes = _equippedEmotesOnHide;
		if (!EmoteUtils.AnyEmoteEquipCapExceeded(emoteDataProvider, in equippedEmotes))
		{
			if (OnSelected != null)
			{
				OnSelected(_equippedEmotesOnHide.ToList());
			}
			else
			{
				_cosmetics.SetEmoteSelections(_equippedEmotesOnHide.ToList());
			}
			HasUnsavedChanges = false;
			onSaveComplete?.Invoke();
			flag = true;
		}
		else
		{
			ShowExceededEquippedEmotesSystemMessage(null, delegate
			{
				HasUnsavedChanges = false;
				Hide();
				onSaveComplete?.Invoke();
			});
		}
		_biLogger.Send(ClientBusinessEventType.EmoteSelectionsModified, new EmoteSelectionsModified
		{
			SelectionMethod = "Manual",
			PreviousSelectedEmotes = _equippedEmotesOnShow.ToArray(),
			UpdatedSelectedEmotes = _equippedEmotesOnHide.ToArray(),
			SaveSuccessful = flag,
			EventTime = DateTime.UtcNow
		});
		_equippedEmotesOnShow = _equippedEmotesOnHide;
		return flag;
	}

	public void SaveAndHide()
	{
		if (Save())
		{
			Hide();
		}
	}

	public void ShowUnsavedChangesSystemMessage(Action cancelButtonClicked, Action onDiscardChangesButtonClicked, Action onSaveAndExitButtonClicked)
	{
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(new SystemMessageManager.SystemMessageButtonData
		{
			Callback = cancelButtonClicked,
			IsConfirm = false,
			Text = _localization.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel")
		});
		list.Add(new SystemMessageManager.SystemMessageButtonData
		{
			Callback = onDiscardChangesButtonClicked,
			IsConfirm = false,
			Text = _localization.GetLocalizedText("MainNav/DeckBuilder/DiscardChanges_Button")
		});
		list.Add(new SystemMessageManager.SystemMessageButtonData
		{
			Callback = onSaveAndExitButtonClicked,
			IsConfirm = true,
			Text = _localization.GetLocalizedText("MainNav/DeckBuilder/SaveAndExit_Button")
		});
		SystemMessageManager.Instance.ShowMessage(_localization.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_ConfirmSavingDeck"), _localization.GetLocalizedText("MainNav/Profile/Emotes/UnsavedChanges_Body"), list);
	}

	public void ShowExceededEquippedEmotesSystemMessage(Action cancelButtonClicked, Action onDiscardChangesButtonClicked)
	{
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(new SystemMessageManager.SystemMessageButtonData
		{
			Callback = cancelButtonClicked,
			IsConfirm = false,
			Text = _localization.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel")
		});
		list.Add(new SystemMessageManager.SystemMessageButtonData
		{
			Callback = onDiscardChangesButtonClicked,
			IsConfirm = false,
			Text = _localization.GetLocalizedText("MainNav/DeckBuilder/DiscardChanges_Button")
		});
		SystemMessageManager.Instance.ShowMessage(_localization.GetLocalizedText("MainNav/Profile/Emotes/MaxEquippedEmoteCapReached_Header"), _localization.GetLocalizedText("MainNav/Profile/Emotes/MaxEquippedEmoteCapReached_Body"), list);
	}

	private EmoteView _instantiateEmote(EmoteData data)
	{
		_assetLookupSystem.Blackboard.Clear();
		EmoteView emoteView = EmoteUtils.InstantiateEmoteView(data, _assetLookupSystem);
		emoteView.Init(data.Id, EmoteUtils.GetPreviewLocKey(data.Id, _assetLookupSystem), EmoteUtils.GetEmoteSfxData(data.Id, _assetLookupSystem));
		emoteView.OnClick += _onEmoteViewClick;
		emoteView.OnHover += _onEmoteViewHover;
		_instantiatedEmotes.Add(data.Id, emoteView);
		return emoteView;
	}

	private void _onEmoteViewHover(string emoteId)
	{
		if (_visibleSampleEmoteId == emoteId)
		{
			return;
		}
		_visibleSampleEmoteId = emoteId;
		for (int i = 0; i < _sampleEmotes.Count; i++)
		{
			UnityEngine.Object.Destroy(_sampleEmotes[i].gameObject);
		}
		_sampleEmotes.Clear();
		if (!_emoteDataProvider.TryGetEmoteData(emoteId, out var emoteData))
		{
			return;
		}
		EmoteView emoteView = EmoteUtils.InstantiateEmoteView(emoteData, _assetLookupSystem);
		SfxData emoteSfxData = EmoteUtils.GetEmoteSfxData(emoteId, _assetLookupSystem);
		emoteView.Init(emoteId, EmoteUtils.GetFullLocKey(emoteId, _assetLookupSystem), emoteSfxData);
		emoteView.SetDisplayOnly(isDisplayOnly: true);
		emoteView.ProfileSkipFirstFrameFade();
		emoteView.SetClickable(emoteSfxData != null);
		_sampleEmotes.Add(emoteView);
		foreach (var associatedEmote in GetAssociatedEmotes(emoteId, _assetLookupSystem))
		{
			EmoteView emoteView2 = EmoteUtils.InstantiateEmoteView(emoteData, _assetLookupSystem);
			emoteView2.Init(associatedEmote.Item1, associatedEmote.Item2, associatedEmote.Item3);
			emoteView2.SetDisplayOnly(isDisplayOnly: true);
			emoteView2.SetClickable(associatedEmote.Item3 != null);
			_sampleEmotes.Add(emoteView2);
		}
		if (emoteData.Entry.Page == EmotePage.Sticker)
		{
			_emoteSelectionPopupView.UpdateSampleStickerEmote(_sampleEmotes[0]);
		}
		else
		{
			_emoteSelectionPopupView.UpdateSamplePhraseEmotes(_sampleEmotes);
		}
	}

	private static IEnumerable<(string, string, SfxData)> GetAssociatedEmotes(string emoteId, AssetLookupSystem assetLookupSystem)
	{
		foreach (string associatedEmoteId in EmoteUtils.GetAssociatedEmoteIds(emoteId, assetLookupSystem))
		{
			yield return (associatedEmoteId, EmoteUtils.GetFullLocKey(associatedEmoteId, assetLookupSystem), EmoteUtils.GetEmoteSfxData(associatedEmoteId, assetLookupSystem));
		}
	}

	private void _onEmoteViewClick(string emoteId)
	{
		bool exceededMaxEquippedEmotesCount = false;
		KeyValuePair<EmotePage, Action> keyValuePair = new KeyValuePair<EmotePage, Action>(EmotePage.Phrase, delegate
		{
			int equippedPhrasesCount = _equippedPhrasesCount;
			_equippedPhrasesCount = EmoteUtils.UpdateEquippedEmote(emoteId, _equippedPhrasesCount, ref _equippedEmotesOnHide, _instantiatedEmotes);
			if (equippedPhrasesCount < _equippedPhrasesCount)
			{
				_instantiatedEmotes[emoteId]?.PlaySfx();
			}
			IEmoteDataProvider emoteDataProvider = _emoteDataProvider;
			ICollection<string> equippedEmotes = _equippedEmotesOnHide;
			exceededMaxEquippedEmotesCount = EmoteUtils.IsEmoteEquipCapExceeded(EmotePage.Phrase, emoteDataProvider, in equippedEmotes);
			_updateNotificationText(exceededMaxEquippedEmotesCount, EmotePage.Phrase, 15);
			HasUnsavedChanges = !_equippedEmotesOnShow.IsEqualTo(_equippedEmotesOnHide);
		});
		KeyValuePair<EmotePage, Action> keyValuePair2 = new KeyValuePair<EmotePage, Action>(EmotePage.Sticker, delegate
		{
			int equippedStickersCount = _equippedStickersCount;
			_equippedStickersCount = EmoteUtils.UpdateEquippedEmote(emoteId, _equippedStickersCount, ref _equippedEmotesOnHide, _instantiatedEmotes);
			if (equippedStickersCount < _equippedStickersCount)
			{
				_instantiatedEmotes[emoteId]?.PlaySfx();
			}
			IEmoteDataProvider emoteDataProvider = _emoteDataProvider;
			ICollection<string> equippedEmotes = _equippedEmotesOnHide;
			exceededMaxEquippedEmotesCount = EmoteUtils.IsEmoteEquipCapExceeded(EmotePage.Sticker, emoteDataProvider, in equippedEmotes);
			_updateNotificationText(exceededMaxEquippedEmotesCount, EmotePage.Sticker, 10);
			HasUnsavedChanges = !_equippedEmotesOnShow.IsEqualTo(_equippedEmotesOnHide);
		});
		EmoteUtils.InvokeActionsOnEmotePageMatch(emoteId, _emoteDataProvider, keyValuePair, keyValuePair2);
		_updateEmotesText(_equippedPhrasesCount, _equippedStickersCount);
		_emoteSelectionPopupView.SetConfirmButtonInteractable(!exceededMaxEquippedEmotesCount);
	}

	private void _resetNotificationText()
	{
		_emoteSelectionPopupView.UpdateNotificationText(_localization.GetLocalizedText("MainNav/Profile/Emotes/UnlockNewEmotes"));
	}

	private void _updateNotificationText(bool maxEquippedCapReached, EmotePage emotePage, int maxEquippedEmotes)
	{
		if (maxEquippedCapReached)
		{
			_emoteSelectionPopupView.UpdateNotificationText(_localization.GetLocalizedText("MainNav/Profile/Emotes/ExceededMaxEquippedEmotes_Header", ("maxEquippedCount", maxEquippedEmotes.ToString()), ("emoteType", emotePage.ToString())), isDefaultTextColor: false);
		}
		else
		{
			_resetNotificationText();
		}
	}

	private void _cleanUpAndHide()
	{
		for (int i = 0; i < _sampleEmotes.Count; i++)
		{
			UnityEngine.Object.Destroy(_sampleEmotes[i].gameObject);
		}
		HasUnsavedChanges = false;
		_sampleEmotes.Clear();
		_emoteSelectionPopupView.gameObject.SetActive(value: false);
	}

	private void _updateEmotesText(int equippedPhrasesCount, int equippedStickersCount)
	{
		_emoteSelectionPopupView.UpdatePhrasesText(_localization.GetLocalizedText("MainNav/Profile/Emotes/EquippedPhrasesCount_Header", ("currentEquippedCount", equippedPhrasesCount.ToString()), ("maxEquippedCount", 15.ToString())), equippedPhrasesCount <= 15);
		_emoteSelectionPopupView.UpdateStickersText(_localization.GetLocalizedText("MainNav/Profile/Emotes/EquippedStickersCount_Header", ("currentEquippedCount", equippedStickersCount.ToString()), ("maxEquippedCount", 10.ToString())), equippedStickersCount <= 10);
	}

	public void SetCallbacks(Action<List<string>> onSelected, Action onHide)
	{
		OnSelected = onSelected;
		OnHide = onHide;
	}
}
