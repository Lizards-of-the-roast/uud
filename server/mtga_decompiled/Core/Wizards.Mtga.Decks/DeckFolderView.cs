using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.Decks;

public class DeckFolderView : MonoBehaviour
{
	public Localize FolderNameLocKey;

	public Localize FolderDescLocKey;

	public Guid FolderId;

	[SerializeField]
	private Localize _validGridDescription;

	[SerializeField]
	private Transform _validGridParent;

	[SerializeField]
	private Localize _invalidGridDescription;

	[SerializeField]
	private Transform _invalidGridParent;

	[SerializeField]
	private CustomButton _newDeckButtonPrefab;

	[SerializeField]
	private Toggle _folderToggle;

	[SerializeField]
	private GameObject _toggleContainer;

	private DeckViewBuilder _deckViewBuilder;

	private List<DeckViewInfo> _decks = new List<DeckViewInfo>();

	private Dictionary<DeckViewInfo, DeckView> _createdDecks = new Dictionary<DeckViewInfo, DeckView>();

	private string _formatForValidation = string.Empty;

	private List<DeckViewInfo> _validDecks;

	private List<DeckViewInfo> _invalidDecks;

	private Action<DeckViewInfo> _onDeckSelected;

	private Action<DeckViewInfo> _onDeckDoubleClicked;

	private Action<DeckViewInfo, string> _onDeckNameEndEdit;

	private Action<Guid> _onFolderToggle;

	private CustomButton _newDeckButtonInstance;

	private Action _createDeckAction;

	private Animator _animator;

	private static readonly int Open = Animator.StringToHash("Open");

	private DeckFolderStatesDataProvider DeckFolderStatesDataProvider => Pantry.Get<DeckFolderStatesDataProvider>();

	public void OnMouseOver_PlaySound()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnDestroy()
	{
		ClearDecks();
		_onDeckSelected = null;
		_onDeckDoubleClicked = null;
		_onDeckNameEndEdit = null;
		_createdDecks = null;
		_validDecks = null;
		_invalidDecks = null;
		_deckViewBuilder = null;
		_decks = null;
		if (_newDeckButtonInstance != null)
		{
			_newDeckButtonInstance.OnMouseover.RemoveListener(OnMouseOver_PlaySound);
			_newDeckButtonInstance.OnClick.RemoveListener(OnCreateDeckButtonClicked);
			_newDeckButtonInstance = null;
		}
	}

	public void Initialize(Action<DeckViewInfo> onDeckSelected, Action<DeckViewInfo> onDeckDoubleClicked, Action<DeckViewInfo, string> onDeckNameEndEdit, Action<Guid> onFolderToggle)
	{
		_deckViewBuilder = Pantry.Get<DeckViewBuilder>();
		_onDeckSelected = onDeckSelected;
		_onDeckDoubleClicked = onDeckDoubleClicked;
		_onDeckNameEndEdit = onDeckNameEndEdit;
		_onFolderToggle = onFolderToggle;
	}

	public void OnEnable()
	{
		UpdateFolderToggle();
	}

	public void OnFolderToggleChanged()
	{
		if (_animator != null)
		{
			_animator.SetBool(Open, _folderToggle.isOn);
		}
		DeckFolderStatesDataProvider.SetFolderState(FolderId.ToString(), _folderToggle.isOn);
		if (!_folderToggle.isOn)
		{
			_onFolderToggle(FolderId);
		}
	}

	public void SetFormat(string format, bool allowUnownedCards, DeckViewSortType sortType, bool isPreconEvent = false)
	{
		UpdateFolderToggle();
		_formatForValidation = format;
		_decks = SortDecks(_decks, _formatForValidation, sortType);
		SeparateDecks(_decks, _formatForValidation, allowUnownedCards, out _validDecks, out _invalidDecks);
		List<DeckViewInfo> invalidDecks = _invalidDecks;
		bool active = invalidDecks != null && invalidDecks.Count > 0;
		UpdateGrid(active: true, _validDecks?.ConvertAll<DeckView>(GetDeckView), _validGridParent);
		UpdateGrid(active, _invalidDecks?.ConvertAll<DeckView>(GetDeckView), _invalidGridParent);
		UpdateDecksFromFormat(_formatForValidation, allowUnownedCards, isPreconEvent);
		_invalidGridDescription.gameObject.UpdateActive(active);
	}

	private List<DeckViewInfo> SortDecks(List<DeckViewInfo> deckViewInfos, string format, DeckViewSortType sortType)
	{
		return DeckViewUtilities.SortDeckViewInfo(deckViewInfos, format, sortType);
	}

	private void UpdateDecksFromFormat(string format, bool allowUnownedCards, bool isPreconEvent = false)
	{
		foreach (KeyValuePair<DeckViewInfo, DeckView> createdDeck in _createdDecks)
		{
			createdDeck.Deconstruct(out var _, out var value);
			value.UpdateDisplayForFormat(format, allowUnownedCards, isPreconEvent);
		}
	}

	public void SetDecks(List<DeckViewInfo> decks, bool allowUnownedCards)
	{
		_decks = decks;
		ClearDecks();
		SeparateDecks(_decks, _formatForValidation, allowUnownedCards, out _validDecks, out _invalidDecks);
		List<DeckViewInfo> validDecks = _validDecks;
		bool num = validDecks != null && validDecks.Count > 0;
		List<DeckViewInfo> invalidDecks = _invalidDecks;
		bool flag = invalidDecks != null && invalidDecks.Count > 0;
		if (num)
		{
			CreateDecks(_validDecks, _validGridParent);
		}
		if (flag)
		{
			CreateDecks(_invalidDecks, _invalidGridParent);
		}
	}

	private DeckView GetDeckView(DeckViewInfo info)
	{
		_createdDecks.TryGetValue(info, out var value);
		return value;
	}

	private void UpdateGrid(bool active, List<DeckView> decks, Transform parent)
	{
		for (int i = 0; i < decks.Count; i++)
		{
			decks[i].gameObject.transform.SetParent(parent);
			decks[i].gameObject.transform.SetSiblingIndex(i + 1);
		}
		parent.gameObject.UpdateActive(active);
	}

	private void UpdateFolderToggle()
	{
		_animator.SetBool(Open, _folderToggle.isOn);
	}

	private static void SeparateDecks(List<DeckViewInfo> decks, string format, bool allowUnowned, out List<DeckViewInfo> validDecks, out List<DeckViewInfo> invalidDecks)
	{
		validDecks = new List<DeckViewInfo>();
		invalidDecks = new List<DeckViewInfo>();
		if (decks == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(format))
		{
			validDecks = decks;
			return;
		}
		foreach (DeckViewInfo deck in decks)
		{
			DeckDisplayInfo validationForFormat = deck.GetValidationForFormat(format);
			((validationForFormat.IsValid || (validationForFormat.IsUnowned && allowUnowned)) ? validDecks : invalidDecks).Add(deck);
		}
	}

	private void CreateDecks(List<DeckViewInfo> deckViewInfos, Transform parent)
	{
		foreach (DeckViewInfo deckViewInfo in deckViewInfos)
		{
			DeckView value = CreateDeck(deckViewInfo, parent);
			_createdDecks.Add(deckViewInfo, value);
		}
	}

	private DeckView CreateDeck(DeckViewInfo deckViewInfo, Transform parent)
	{
		DeckView deckView = _deckViewBuilder.CreateDeckView(deckViewInfo, parent);
		deckView.SetDeckOnClick(_onDeckSelected);
		deckView.SetDeckOnDoubleClick(_onDeckDoubleClicked);
		deckView.SetOnDeckNameEndEdit(_onDeckNameEndEdit);
		return deckView;
	}

	public DeckViewInfo GetDeckViewInfo(string deckId)
	{
		return _decks.FirstOrDefault((DeckViewInfo d) => d.deckId.ToString() == deckId);
	}

	public DeckView SelectDeck(DeckViewInfo selectedViewInfo)
	{
		DeckView result = null;
		foreach (var (deckViewInfo2, deckView2) in _createdDecks)
		{
			deckView2.SetIsSelected(deckViewInfo2 == selectedViewInfo);
			if (deckViewInfo2 == selectedViewInfo)
			{
				result = deckView2;
			}
		}
		return result;
	}

	public void ClearDecks()
	{
		foreach (DeckView value in _createdDecks.Values)
		{
			_deckViewBuilder.ReleaseDeckView(value);
		}
		_createdDecks.Clear();
	}

	public void ShowCreateDeckButton(Action action)
	{
		_createDeckAction = action;
		if (_newDeckButtonInstance == null)
		{
			_newDeckButtonInstance = UnityEngine.Object.Instantiate(_newDeckButtonPrefab, _validGridParent.transform, worldPositionStays: true);
			_newDeckButtonInstance.transform.ZeroOut();
			_newDeckButtonInstance.transform.SetSiblingIndex(0);
			_newDeckButtonInstance.OnMouseover.AddListener(OnMouseOver_PlaySound);
			_newDeckButtonInstance.OnClick.AddListener(OnCreateDeckButtonClicked);
		}
		else
		{
			_newDeckButtonInstance.gameObject.SetActive(value: true);
		}
	}

	public void HideCreateDeckButton()
	{
		if (!(_newDeckButtonInstance == null))
		{
			_newDeckButtonInstance.gameObject.SetActive(value: false);
		}
	}

	private void OnCreateDeckButtonClicked()
	{
		_createDeckAction?.Invoke();
	}

	public void SetFolderOpenState(bool isOpen)
	{
		_folderToggle.isOn = isOpen;
		_toggleContainer.SetActive(isOpen);
	}

	public bool GetFolderOpenState()
	{
		return _folderToggle.isOn;
	}
}
