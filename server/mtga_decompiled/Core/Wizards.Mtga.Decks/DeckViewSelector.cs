using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.Shared;
using UnityEngine;

namespace Wizards.Mtga.Decks;

public class DeckViewSelector : MonoBehaviour
{
	private NetDeckFolderDataProvider _netDeckFolderDataProvider;

	private List<DeckFolderView> _deckFolders = new List<DeckFolderView>();

	private DeckViewSortType _sortType;

	[SerializeField]
	private Transform _folderParent;

	private DeckView _selectedDeckView;

	private Action<DeckViewInfo> _onDeckDeselected;

	private Action<DeckViewInfo> _onDeckSelected;

	private Action<DeckViewInfo> _onDeckDoubleClicked;

	private Action<DeckViewInfo, string> _onDeckNameEndEdit;

	private Action _createDeckAction;

	private const string MyDeckLocString = "MainNav/DeckManager/MyDecks_Folder";

	private DeckFolderView _myDecksFolderView;

	public DeckFolderView _deckFolderViewPrefab;

	public DeckFolderView _deckFolderViewNoFolderPrefab;

	private DeckFolderStatesDataProvider _deckFolderStatesDataProvider;

	private void Awake()
	{
		_netDeckFolderDataProvider = Pantry.Get<NetDeckFolderDataProvider>();
		_deckFolderStatesDataProvider = Pantry.Get<DeckFolderStatesDataProvider>();
	}

	private void OnDestroy()
	{
		_deckFolderStatesDataProvider.Save();
		ClearDecks();
		_onDeckSelected = null;
		_onDeckDoubleClicked = null;
		_onDeckNameEndEdit = null;
		_selectedDeckView = null;
	}

	public void Initialize(Action<DeckViewInfo> onDeckSelected, Action<DeckViewInfo> onDeckDoubleClicked, Action<DeckViewInfo, string> onDeckNameEndEdit = null, bool simpleSelect = false, bool? myDecksDefaultOpen = null)
	{
		_onDeckSelected = onDeckSelected;
		_onDeckDoubleClicked = onDeckDoubleClicked;
		_onDeckNameEndEdit = onDeckNameEndEdit;
		if (simpleSelect)
		{
			DeckFolderView deckFolderView = UnityEngine.Object.Instantiate(_deckFolderViewNoFolderPrefab, _folderParent);
			deckFolderView.Initialize(SelectDeck, _onDeckDoubleClicked, _onDeckNameEndEdit, OnFolderToggle);
			deckFolderView.FolderId = Guid.Empty;
			deckFolderView.SetFolderOpenState(isOpen: true);
			_myDecksFolderView = deckFolderView;
			_deckFolders.Add(deckFolderView);
			return;
		}
		DeckFolderView deckFolderView2 = UnityEngine.Object.Instantiate(_deckFolderViewPrefab, _folderParent);
		deckFolderView2.Initialize(SelectDeck, _onDeckDoubleClicked, _onDeckNameEndEdit, OnFolderToggle);
		deckFolderView2.ShowCreateDeckButton(_createDeckAction);
		deckFolderView2.FolderNameLocKey.SetText("MainNav/DeckManager/MyDecks_Folder");
		deckFolderView2.FolderId = Guid.Empty;
		if (myDecksDefaultOpen.HasValue)
		{
			deckFolderView2.SetFolderOpenState(myDecksDefaultOpen.Value);
		}
		_myDecksFolderView = deckFolderView2;
		_deckFolders.Add(deckFolderView2);
		foreach (NetDeckFolder netDeckFolder in _netDeckFolderDataProvider.GetNetDeckFolders())
		{
			DeckFolderView deckFolderView3 = UnityEngine.Object.Instantiate(_deckFolderViewPrefab, _folderParent);
			deckFolderView3.Initialize(SelectDeck, _onDeckDoubleClicked, _onDeckNameEndEdit, OnFolderToggle);
			deckFolderView3.FolderId = netDeckFolder.Id;
			deckFolderView3.FolderNameLocKey.SetText(netDeckFolder.FolderNameLocKey);
			deckFolderView3.FolderDescLocKey.SetText(netDeckFolder.FolderDescLocKey);
			_deckFolders.Add(deckFolderView3);
		}
	}

	public void SetFormat(string format, bool allowUnownedCards, bool isPreconEvent = false)
	{
		foreach (DeckFolderView deckFolder in _deckFolders)
		{
			deckFolder.SetFormat(format, allowUnownedCards, _sortType, isPreconEvent);
		}
	}

	public void SetSort(DeckViewSortType type)
	{
		_sortType = type;
	}

	public void SetDecks(List<DeckViewInfo> decks, bool allowUnownedCards)
	{
		_selectedDeckView = null;
		foreach (DeckFolderView deckFolderView in _deckFolders)
		{
			deckFolderView.SetFolderOpenState(_deckFolderStatesDataProvider.GetFolderState(deckFolderView.FolderId.ToString()));
			List<DeckViewInfo> list = decks.Where((DeckViewInfo d) => d.NetDeckFolderId == deckFolderView.FolderId || (!d.NetDeckFolderId.HasValue && deckFolderView.FolderId == Guid.Empty)).ToList();
			if (deckFolderView.FolderId != Guid.Empty)
			{
				deckFolderView.gameObject.SetActive(list.Count != 0);
			}
			deckFolderView.SetDecks(list, allowUnownedCards);
		}
	}

	public void SelectDeck(string deckId)
	{
		foreach (DeckFolderView deckFolder in _deckFolders)
		{
			DeckViewInfo deckViewInfo = deckFolder.GetDeckViewInfo(deckId);
			if (deckViewInfo != null)
			{
				SelectDeck(deckViewInfo);
				break;
			}
		}
	}

	public void SelectDeck(DeckViewInfo selectedViewInfo)
	{
		if ((bool)_selectedDeckView)
		{
			_selectedDeckView.SetIsSelected(isSelected: false);
		}
		foreach (DeckFolderView deckFolder in _deckFolders)
		{
			DeckView deckView = deckFolder.SelectDeck(selectedViewInfo);
			if (deckView != null)
			{
				_selectedDeckView = deckView;
			}
		}
		_onDeckSelected?.Invoke(selectedViewInfo);
	}

	public void DeselectDeck()
	{
		if ((bool)_selectedDeckView)
		{
			_selectedDeckView.SetIsSelected(isSelected: false);
		}
		_selectedDeckView = null;
		_onDeckSelected?.Invoke(null);
	}

	private void OnFolderToggle(Guid folderId)
	{
		DeckFolderView deckFolderView = _deckFolders.Find((DeckFolderView folder) => folder.FolderId == folderId);
		string deckId = _selectedDeckView?.GetDeckId().ToString();
		if (deckFolderView.GetDeckViewInfo(deckId) != null)
		{
			DeselectDeck();
		}
	}

	public void ClearDecks()
	{
		foreach (DeckFolderView deckFolder in _deckFolders)
		{
			deckFolder.ClearDecks();
		}
	}

	public void ShowCreateDeckButton(Action action)
	{
		_myDecksFolderView.ShowCreateDeckButton(action);
	}

	public void HideCreateDeckButton()
	{
		_myDecksFolderView.HideCreateDeckButton();
	}
}
