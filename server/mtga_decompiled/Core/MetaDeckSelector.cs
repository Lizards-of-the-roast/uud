using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetLookupTree;
using Pooling;
using UnityEngine;
using Wizards.Arena.Enums.Card;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class MetaDeckSelector : MonoBehaviour
{
	[SerializeField]
	private GameObject PlayQueueDeckSelectTabs;

	[SerializeField]
	private GameObject _PlayQueueDeckSelectTab_Standard;

	[SerializeField]
	private GameObject _PlayQueueDeckSelectTab_Historic;

	public Action<MetaDeckView> DeckView_OnClick;

	public Action<MetaDeckView, string> DeckView_OnNameChanged;

	public bool ShowTooltip = true;

	[SerializeField]
	private Localize topGridDescription;

	[SerializeField]
	private Transform topGridParent;

	[SerializeField]
	private Localize bottomGridDescription;

	[SerializeField]
	private Transform bottomGridParent;

	public CustomButton BackgroundButton;

	private MetaDeckView _selectedDeckView;

	private List<MetaDeckView> _allDeckViews = new List<MetaDeckView>();

	private Coroutine _runningSetDecks;

	private AssetLookupSystem _assetLookupSystem;

	private IUnityObjectPool _objectPool;

	private BILogger _biLogger;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	public Transform DeckViewParent => topGridParent;

	public MetaDeckView SelectedDeckView => _selectedDeckView;

	public event Action<string> TabClicked;

	public void Init(AssetLookupSystem assetLookupSystem, BILogger biLogger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_assetLookupSystem = assetLookupSystem;
		_biLogger = biLogger;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_objectPool = Pantry.Get<IUnityObjectPool>();
	}

	public void SetUpTabsOnSelectorShow(EventContext eventContext)
	{
		bool flag = eventContext.PlayerEvent.EventInfo.InternalEventName.Equals("Play");
		bool flag2 = eventContext.PlayerEvent.EventInfo.InternalEventName.Equals("Historic_Play");
		bool flag3 = flag || flag2;
		PlayQueueDeckSelectTabs.SetActive(flag3);
		if (flag3)
		{
			SetTabState(flag);
		}
	}

	private void SetTabState(bool standard)
	{
		_PlayQueueDeckSelectTab_Standard.SetActive(standard);
		_PlayQueueDeckSelectTab_Historic.SetActive(!standard);
		this.TabClicked?.Invoke(standard ? "Play" : "Historic_Play");
	}

	public void StandardTabFilterClicked()
	{
		SetTabState(standard: true);
	}

	public void AllTabFilterClicked()
	{
		SetTabState(standard: false);
	}

	public void SetDecks(List<DeckDisplayInfo> topGridDecks, List<DeckDisplayInfo> bottomGridDecks = null, DeckFormat format = null, string selectedDeckId = null, Action selectedCallback = null)
	{
		ClearDecks();
		bottomGridDescription.gameObject.SetActive(bottomGridDecks != null && bottomGridDecks.Count != 0);
		_runningSetDecks = StartCoroutine(Coroutine_SetDecks(topGridDecks, bottomGridDecks, selectedDeckId, selectedCallback));
	}

	private IEnumerator Coroutine_SetDecks(List<DeckDisplayInfo> topGridDecks, List<DeckDisplayInfo> bottomGridDecks, string selectedDeckId, Action selectedCallback)
	{
		int index = 1;
		CreateGridDecks(topGridDecks, topGridParent);
		if (bottomGridDecks != null && bottomGridDecks.Count > 0)
		{
			topGridDescription.gameObject.UpdateActive(active: true);
			bottomGridDescription.gameObject.UpdateActive(active: true);
			CreateGridDecks(bottomGridDecks, bottomGridParent);
		}
		else
		{
			topGridDescription.gameObject.UpdateActive(active: false);
			bottomGridDescription.gameObject.UpdateActive(active: false);
		}
		_runningSetDecks = null;
		yield return new WaitForEndOfFrame();
		selectedCallback?.Invoke();
		void CreateGridDecks(List<DeckDisplayInfo> displayDecks, Transform parent)
		{
			index = parent.childCount;
			foreach (DeckDisplayInfo displayDeck in displayDecks)
			{
				_ = displayDeck.ValidationResult;
				MetaDeckView deckView = DeckBoxUtil.CreateDeckBox(_assetLookupSystem, _objectPool, _biLogger);
				deckView.transform.SetParent(parent);
				deckView.transform.SetSiblingIndex(index++);
				deckView.transform.ZeroOut();
				deckView.Init(_cardDatabase, _cardViewBuilder, displayDeck.Deck);
				deckView.ColorChallengeEventLock = displayDeck.ColorChallengeEventLock;
				bool flag = DeckView_OnClick != null;
				deckView.Button.Interactable = flag;
				if (flag)
				{
					deckView.Button.OnClick.AddListener(delegate
					{
						DeckView_OnClick_Wrapper(deckView);
					});
				}
				bool flag2 = DeckView_OnNameChanged != null;
				deckView.SetNameIsEditable(flag2);
				if (flag2)
				{
					deckView.NameText.onEndEdit.AddListener(delegate(string s)
					{
						DeckView_OnNameChanged(deckView, s);
					});
				}
				if (ShowTooltip)
				{
					string toolTipForDeck = DeckViewUtilities.GetToolTipForDeck(displayDeck);
					deckView.SetToolTipText(toolTipForDeck);
				}
				else
				{
					deckView.SetToolTipText(string.Empty);
				}
				deckView.SetIsValid(displayDeck.DisplayState, displayDeck.ValidationResult);
				if (deckView.Model.Id.ToString() == selectedDeckId)
				{
					UpdateSelectedDeckView(deckView);
				}
				deckView.RootAnimator.SetBool("Historic", displayDeck.UseHistoricLabel);
				deckView.RootAnimator.SetBool("Favorited", displayDeck.Deck.Summary.IsFavorite);
				deckView.RootAnimator.SetBool("Locked", !string.IsNullOrWhiteSpace(displayDeck.ColorChallengeEventLock));
				_allDeckViews.Add(deckView);
			}
		}
	}

	public void ClearDecks()
	{
		if (_runningSetDecks != null)
		{
			StopCoroutine(_runningSetDecks);
			_runningSetDecks = null;
		}
		_selectedDeckView = null;
		foreach (MetaDeckView allDeckView in _allDeckViews)
		{
			allDeckView.Button.OnClick.RemoveAllListeners();
			allDeckView.NameText.onEndEdit.RemoveAllListeners();
			DeckBoxUtil.DestroyDeckBox(allDeckView, _objectPool);
		}
		_allDeckViews.Clear();
	}

	public void SelectDeck(string id)
	{
		UpdateSelectedDeckView(_allDeckViews.FirstOrDefault((MetaDeckView dv) => dv.Model.Id.ToString() == id));
	}

	private static StringBuilder ConstructToolTipFromUnownedBasedOffRarity(Rarity rarity, Dictionary<Rarity, int> UnownedCards, IClientLocProvider localizationManager)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (UnownedCards.TryGetValue(rarity, out var value))
		{
			stringBuilder.Append(localizationManager.GetLocalizedText(GetWildCardLocalizationKeyForRarity(rarity)));
			stringBuilder.Append(" " + value);
			stringBuilder.Append(Environment.NewLine);
		}
		return stringBuilder;
	}

	private static string GetWildCardLocalizationKeyForRarity(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => "MainNav/ConstructedDeckSelect/Tooltip_Common", 
			Rarity.Uncommon => "MainNav/ConstructedDeckSelect/Tooltip_Uncommon", 
			Rarity.Rare => "MainNav/ConstructedDeckSelect/Tooltip_Rare", 
			Rarity.Mythic => "MainNav/ConstructedDeckSelect/Tooltip_Mythic", 
			_ => "", 
		};
	}

	private void DeckView_OnClick_Wrapper(MetaDeckView deckView)
	{
		UpdateSelectedDeckView(deckView);
		DeckView_OnClick(deckView);
	}

	private void UpdateSelectedDeckView(MetaDeckView deckView)
	{
		if (_selectedDeckView != null)
		{
			_selectedDeckView.SetIsSelected(isSelected: false);
		}
		_selectedDeckView = deckView;
		if (_selectedDeckView != null)
		{
			_selectedDeckView.SetIsSelected(isSelected: true);
		}
	}
}
