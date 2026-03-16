using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Loc.CachingPatterns;
using Wotc.Mtgo.Gre.External.Messaging;

public class WishGUI : MonoBehaviour
{
	private const int MAX_RESULTS = 100;

	private GUIStyle m_flatColorTextureStyle;

	private GUIStyle m_scrollViewStyle;

	private GUIStyle m_filterTextFieldStyle;

	private GUIStyle m_filterOptionsStyle;

	private GUIStyle m_cardButtonsStyle;

	private int fontSize;

	private List<CardPrintingData> _filteredCards;

	private Vector2 _scroll;

	private bool _focus;

	private int _index;

	public static string TitleFilter = "";

	public static string TypeFilter = "";

	public static string SetFilter = "";

	public static string ArtIdFilter = "";

	public static string BodyFilter = "";

	public static string LinkedFacesFilter = "";

	public static string DigitalSetReleaseFilter = "";

	public static bool _forceFilter = false;

	private bool _canCancel;

	private ICardDatabaseAdapter _cardDatabase;

	private CardDatabaseSearcher _searcher;

	private ICachingPattern<(uint, bool), string> _textCache = new DictionaryCache<(uint, bool), string>(2500);

	private int _currentResultsPage;

	private static readonly int _pageSize = 100;

	private int _startingItemNumber = 1;

	private int _endingItemNumber = 100;

	private bool _showAdditionalFilters;

	private bool _searchIncludesNonFullOnCards;

	private string _lastTitleFilter = "";

	private string _lastTypeFilter = "";

	private string _lastSetFilter = "";

	private string _lastArtIdFilter = "";

	private string _lastBodyFilter = "";

	private string _lastLinkedFacesFilter = "";

	private string _lastDigitalSetFilter = "";

	private bool _lastSearchIncludesNonFullOnCards;

	public event Action<uint> WishSelectedHandlers;

	public void Init(bool canCancel, ICardDatabaseAdapter cardDatabase, Func<CardPrintingData, bool> globalFilter = null)
	{
		_canCancel = canCancel;
		_cardDatabase = cardDatabase;
		_filteredCards = new List<CardPrintingData>();
		_focus = true;
		_index = 0;
		_forceFilter = true;
		_searcher = new CardDatabaseSearcher(cardDatabase);
		_searcher.SetIgnoreSpecificCards(ignoreWildCards: true, ignoreNonPrimaryCards: true);
	}

	private void Update()
	{
		bool flag = false;
		if (_lastTitleFilter != TitleFilter)
		{
			flag = true;
			if (string.IsNullOrEmpty(TitleFilter))
			{
				_searcher.ClearTitleFilter();
			}
			else
			{
				_searcher.ChangeTitleFilter(TitleFilter);
			}
		}
		if (_lastTypeFilter != TypeFilter)
		{
			flag = true;
			List<CardType> baseTypes;
			List<SuperType> superTypes;
			List<SubType> subTypes;
			if (string.IsNullOrEmpty(TypeFilter))
			{
				_searcher.ClearTypeFilter();
			}
			else if (TryParseTypeFilter(TypeFilter, out baseTypes, out superTypes, out subTypes))
			{
				_searcher.ChangeTypeFilter(baseTypes);
				_searcher.ChangeSuperTypeFilter(superTypes);
				_searcher.ChangeSubTypeFilter(subTypes);
			}
		}
		if (_lastSetFilter != SetFilter)
		{
			flag = true;
			_searcher.ChangeSetFilter(ParseSetFilter(SetFilter));
		}
		if (_lastArtIdFilter != ArtIdFilter)
		{
			flag = true;
			if (uint.TryParse(ArtIdFilter, out var result))
			{
				_searcher.ChangeArtIdFilter(result);
			}
			else
			{
				_searcher.ChangeArtIdFilter(0u);
			}
		}
		if (_lastBodyFilter != BodyFilter)
		{
			flag = true;
			if (string.IsNullOrEmpty(BodyFilter))
			{
				_searcher.ClearBodyFilter();
			}
			else
			{
				_searcher.ChangeBodyFilter(BodyFilter);
			}
		}
		if (_lastLinkedFacesFilter != LinkedFacesFilter)
		{
			flag = true;
			if (string.IsNullOrEmpty(LinkedFacesFilter))
			{
				_searcher.ClearLinkedFaceFilter();
			}
			else
			{
				_searcher.ChangeLinkedFaceFilter(LinkedFacesFilter);
			}
		}
		if (_lastDigitalSetFilter != DigitalSetReleaseFilter)
		{
			flag = true;
			if (string.IsNullOrEmpty(DigitalSetReleaseFilter))
			{
				_searcher.ClearDigitalSetFilter();
			}
			else
			{
				_searcher.ChangeDigitalSetFilter(ParseDigitalSetFilter(DigitalSetReleaseFilter));
			}
		}
		if (_lastSearchIncludesNonFullOnCards != _searchIncludesNonFullOnCards)
		{
			flag = true;
			_searcher.SetIgnoreSpecificCards(ignoreWildCards: true, !_searchIncludesNonFullOnCards);
		}
		if (_forceFilter)
		{
			flag = true;
		}
		if (flag)
		{
			_forceFilter = false;
			_currentResultsPage = 0;
			UpdateFilteredCards(1, (_pageSize > _searcher.CurrentList.Count) ? _searcher.CurrentList.Count : _pageSize);
		}
	}

	private void UpdateFilteredCards(int startingItem, int endingItem)
	{
		_index = 0;
		_startingItemNumber = startingItem;
		_endingItemNumber = endingItem;
		int count = ((endingItem == startingItem) ? 1 : (endingItem - startingItem + 1));
		_filteredCards.Clear();
		if (_endingItemNumber > 0)
		{
			_filteredCards.AddRange(_searcher.CurrentList.GetRange(_startingItemNumber - 1, count));
			_filteredCards.Sort(CompareEnglishCardTitles);
		}
		_lastTitleFilter = TitleFilter;
		_lastTypeFilter = TypeFilter;
		_lastSetFilter = SetFilter;
		_lastArtIdFilter = ArtIdFilter;
		_lastBodyFilter = BodyFilter;
		_lastLinkedFacesFilter = LinkedFacesFilter;
		_lastDigitalSetFilter = DigitalSetReleaseFilter;
		_lastSearchIncludesNonFullOnCards = _searchIncludesNonFullOnCards;
	}

	private void ClearAllFilters()
	{
		TitleFilter = "";
		TypeFilter = "";
		SetFilter = "";
		ArtIdFilter = "";
		BodyFilter = "";
		LinkedFacesFilter = "";
		DigitalSetReleaseFilter = "";
		_searcher.ClearFilters();
	}

	private bool TryParseTypeFilter(string filter, out List<CardType> baseTypes, out List<SuperType> superTypes, out List<SubType> subTypes)
	{
		string[] array = filter.ToLower().Split(' ', ',');
		bool result = false;
		baseTypes = new List<CardType>();
		superTypes = new List<SuperType>();
		subTypes = new List<SubType>();
		string[] array2 = array;
		foreach (string value in array2)
		{
			SuperType result3;
			SubType result4;
			if (Enum.TryParse<CardType>(value, ignoreCase: true, out var result2))
			{
				baseTypes.Add(result2);
				result = true;
			}
			else if (Enum.TryParse<SuperType>(value, ignoreCase: true, out result3))
			{
				superTypes.Add(result3);
				result = true;
			}
			else if (Enum.TryParse<SubType>(value, ignoreCase: true, out result4))
			{
				subTypes.Add(result4);
				result = true;
			}
		}
		return result;
	}

	private List<string> ParseSetFilter(string filter)
	{
		List<string> list = new List<string>();
		string[] array = filter.ToUpper().Split(' ', ',');
		foreach (string item in array)
		{
			if (_cardDatabase.DatabaseUtilities.SetsInDatabase().Contains(item))
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	private List<string> ParseDigitalSetFilter(string filter)
	{
		List<string> list = new List<string>();
		string[] array = filter.ToUpper().Split(' ', ',');
		foreach (string item in array)
		{
			list.Add(item);
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	private int CompareEnglishCardTitles(CardPrintingData lhs, CardPrintingData rhs)
	{
		string englishLoc = GetEnglishLoc(lhs.TitleId);
		string englishLoc2 = GetEnglishLoc(rhs.TitleId);
		return string.CompareOrdinal(englishLoc, englishLoc2);
	}

	private string GetEnglishLoc(uint locId)
	{
		bool num = Languages.CurrentLanguage != "en-US";
		(uint, bool) key = (locId, false);
		if (!num || !_textCache.TryGetCached(key, out var value))
		{
			value = _cardDatabase.GreLocProvider.GetLocalizedText(locId, "en-US", formatted: false);
			_textCache.SetCached(key, value);
		}
		return value;
	}

	private void OnGUI()
	{
		GUI.matrix = (PlatformUtils.IsHandheld() ? Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.height / 800f, (float)Screen.height / 800f, 1f)) : Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.height / 1080f, (float)Screen.height / 1080f, 1f)));
		if (m_flatColorTextureStyle == null)
		{
			m_flatColorTextureStyle = new GUIStyle(GUI.skin.box)
			{
				normal = 
				{
					background = FDWatcherGUI.CreateSolidColorTexture(2, 2, UnityEngine.Color.grey)
				},
				alignment = TextAnchor.MiddleLeft
			};
		}
		if (m_scrollViewStyle == null)
		{
			m_scrollViewStyle = new GUIStyle(GUI.skin.scrollView)
			{
				normal = 
				{
					background = FDWatcherGUI.CreateSolidColorTexture(2, 2, UnityEngine.Color.white)
				},
				alignment = TextAnchor.MiddleLeft
			};
		}
		if (Event.current.type == EventType.KeyDown)
		{
			if (Event.current.keyCode == KeyCode.DownArrow && _index + 1 < _filteredCards.Count)
			{
				_index++;
				Event.current = null;
			}
			else if (Event.current.keyCode == KeyCode.UpArrow && _index > 0)
			{
				_index--;
				Event.current = null;
			}
			else if (Event.current.keyCode == KeyCode.Return)
			{
				if (0 <= _index && _index < _filteredCards.Count)
				{
					this.WishSelectedHandlers?.Invoke(_filteredCards[_index].GrpId);
					Event.current = null;
				}
			}
			else if (Event.current.keyCode == KeyCode.Escape && _canCancel)
			{
				this.WishSelectedHandlers?.Invoke(0u);
			}
		}
		int num;
		int value;
		if (PlatformUtils.IsHandheld())
		{
			if (PlatformUtils.IsAspectRatio4x3())
			{
				value = (int)((double)Screen.safeArea.width * 0.45);
				value = Mathf.Clamp(value, 600, 1000);
				num = (int)((double)Screen.safeArea.height * 0.3);
				fontSize = Screen.width / 120;
				fontSize = Mathf.Clamp(fontSize, 18, 24);
			}
			else
			{
				value = (int)((double)Screen.safeArea.width * 0.5);
				value = Mathf.Clamp(value, 1000, 1600);
				num = (int)((double)Screen.safeArea.height * 0.3);
				fontSize = Screen.width / 75;
				fontSize = Mathf.Clamp(fontSize, 22, 45);
			}
			GUILayout.BeginArea(new Rect((int)(Screen.safeArea.x + Screen.safeArea.width * 0.05f), (int)(Screen.safeArea.y * 1.05f), value, (int)((double)Screen.safeArea.height * 0.5)));
		}
		else
		{
			value = 1152;
			num = 972;
			GUILayout.BeginArea(new Rect(384f, 54f, value, num));
			fontSize = 22;
		}
		GUILayout.BeginVertical(GUILayout.MaxWidth((float)value * 0.9f));
		GUI.color = UnityEngine.Color.white;
		GUILayout.BeginVertical(m_flatColorTextureStyle);
		CreateFilters();
		_scroll = GUILayout.BeginScrollView(_scroll, m_scrollViewStyle, GUILayout.MaxHeight(num));
		GetScrollViewFillerValues(_scroll.y, _filteredCards.Count, 23f, 100, out var firstVisibleIndex, out var lastVisibleIndex, out var beginningFiller, out var endingFiller);
		GUILayout.Space(beginningFiller + 3f);
		CreateCardButtons(firstVisibleIndex, lastVisibleIndex);
		GUILayout.Space(endingFiller);
		GUILayout.EndScrollView();
		GUI.backgroundColor = UnityEngine.Color.grey;
		GUI.contentColor = UnityEngine.Color.white;
		GUILayout.BeginHorizontal(m_flatColorTextureStyle);
		CreateNavButtons();
		m_filterOptionsStyle.alignment = TextAnchor.MiddleLeft;
		GUILayout.Space(100f);
		_searchIncludesNonFullOnCards = GUILayout.Toggle(_searchIncludesNonFullOnCards, "Search includes Non-'FullOn Cards'", m_filterOptionsStyle);
		GUILayout.EndHorizontal();
		if (_canCancel && GUILayout.Button("Cancel", m_cardButtonsStyle, GUILayout.MaxWidth((float)value * 0.9f)))
		{
			this.WishSelectedHandlers?.Invoke(0u);
		}
		GUILayout.EndVertical();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (_focus)
		{
			GUI.FocusControl("Filter");
			_focus = false;
		}
		GUI.matrix = Matrix4x4.identity;
	}

	private void CreateFilters()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label)
		{
			fontSize = fontSize
		};
		GUILayout.BeginHorizontal();
		GUILayout.Label("Title", style);
		GUILayout.Label("Type", style, GUILayout.Width(200f));
		GUILayout.Label("Set", style, GUILayout.Width(100f));
		GUILayout.Label("ArtId", style, GUILayout.Width(100f));
		GUILayout.EndHorizontal();
		GUI.SetNextControlName("Filter (Title, Type, Set)");
		GUILayout.BeginHorizontal();
		if (m_filterTextFieldStyle == null)
		{
			m_filterTextFieldStyle = new GUIStyle(GUI.skin.textField)
			{
				alignment = TextAnchor.MiddleLeft,
				fontSize = fontSize
			};
		}
		GUI.SetNextControlName("Filter");
		TitleFilter = GUILayout.TextField(TitleFilter, m_filterTextFieldStyle);
		TypeFilter = GUILayout.TextField(TypeFilter, m_filterTextFieldStyle, GUILayout.Width(200f));
		SetFilter = GUILayout.TextField(SetFilter, m_filterTextFieldStyle, GUILayout.Width(100f));
		ArtIdFilter = GUILayout.TextField(ArtIdFilter, m_filterTextFieldStyle, GUILayout.Width(80f));
		GUI.contentColor = UnityEngine.Color.red;
		if (GUILayout.Button("X", GUILayout.Width(20f), GUILayout.ExpandHeight(expand: true)))
		{
			ClearAllFilters();
		}
		GUI.contentColor = UnityEngine.Color.white;
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Ability Text", style, GUILayout.Width(500f));
		GUILayout.Label("Backside/Child/Token", style);
		GUILayout.Label("DIgital Set", style, GUILayout.Width(200f));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		BodyFilter = GUILayout.TextField(BodyFilter, m_filterTextFieldStyle, GUILayout.Width(500f));
		LinkedFacesFilter = GUILayout.TextField(LinkedFacesFilter, m_filterTextFieldStyle);
		DigitalSetReleaseFilter = GUILayout.TextField(DigitalSetReleaseFilter, m_filterTextFieldStyle, GUILayout.Width(200f));
		GUILayout.EndHorizontal();
	}

	private void CreateNavButtons()
	{
		if (m_filterOptionsStyle == null)
		{
			m_filterOptionsStyle = new GUIStyle(GUI.skin.toggle)
			{
				fontSize = fontSize,
				alignment = TextAnchor.MiddleRight
			};
		}
		GUIStyle style = new GUIStyle(GUI.skin.textArea)
		{
			fontSize = fontSize
		};
		GUI.contentColor = UnityEngine.Color.white;
		if (GUILayout.Button("<<", GUILayout.Width(50f)) && _currentResultsPage > 0)
		{
			_currentResultsPage--;
			_startingItemNumber = _currentResultsPage * _pageSize + 1;
			_endingItemNumber = _startingItemNumber + _pageSize - 1;
			UpdateFilteredCards(_startingItemNumber, _endingItemNumber);
		}
		GUILayout.Label((_filteredCards.Count == 0) ? "NO RESULTS" : $"{_startingItemNumber} to {_endingItemNumber} of {_searcher.CurrentList.Count}", style, GUILayout.ExpandWidth(expand: true), GUILayout.Width(300f));
		if (GUILayout.Button(">>", GUILayout.Width(50f)) && _endingItemNumber < _searcher.CurrentList.Count)
		{
			_currentResultsPage++;
			_startingItemNumber += _pageSize;
			_endingItemNumber = _startingItemNumber + _pageSize - 1;
			if (_endingItemNumber > _searcher.CurrentList.Count)
			{
				_endingItemNumber = _searcher.CurrentList.Count;
			}
			UpdateFilteredCards(_startingItemNumber, _endingItemNumber);
		}
	}

	private void CreateCardButtons(int firstVisible, int lastVisible)
	{
		if (m_cardButtonsStyle == null)
		{
			m_cardButtonsStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = fontSize,
				alignment = TextAnchor.MiddleLeft
			};
		}
		for (int i = firstVisible; i <= lastVisible; i++)
		{
			CardPrintingData cardPrintingData = _filteredCards[i];
			GUI.contentColor = ((i == _index) ? UnityEngine.Color.green : (cardPrintingData.HasParseFailure ? UnityEngine.Color.red : UnityEngine.Color.black));
			GUI.backgroundColor = ButtonBackgroundColor(cardPrintingData);
			string arg = string.Format("{0} {1} - {2}", string.Join(" ", cardPrintingData.Supertypes.Select(EnumExtensions.EnumCleanName)), string.Join(" ", cardPrintingData.Types.Select(EnumExtensions.EnumCleanName)), string.Join(" ", cardPrintingData.Subtypes.Select(EnumExtensions.EnumCleanName)));
			if (GUILayout.Button(string.Format("{0,-35} {1,25}", string.Format("{0} ({1})", _cardDatabase.GreLocProvider.GetLocalizedText(cardPrintingData.TitleId, "en-US", formatted: false), cardPrintingData.GrpId), $"{arg} | {cardPrintingData.ExpansionCode}{(string.IsNullOrEmpty(cardPrintingData.DigitalReleaseSet) ? string.Empty : $" ({cardPrintingData.DigitalReleaseSet})")}"), m_cardButtonsStyle))
			{
				this.WishSelectedHandlers?.Invoke(_filteredCards[i].GrpId);
				Event.current = null;
			}
		}
	}

	private static UnityEngine.Color ButtonBackgroundColor(CardPrintingData cardData)
	{
		if (cardData.Types.Contains(CardType.Artifact) || cardData.Colors.Count == 0)
		{
			return UnityEngine.Color.grey;
		}
		if (cardData.Colors.Count > 1)
		{
			return UnityEngine.Color.yellow;
		}
		return cardData.Colors[0] switch
		{
			CardColor.White => UnityEngine.Color.white, 
			CardColor.Blue => UnityEngine.Color.blue, 
			CardColor.Black => UnityEngine.Color.black, 
			CardColor.Red => UnityEngine.Color.red, 
			CardColor.Green => UnityEngine.Color.green, 
			_ => UnityEngine.Color.grey, 
		};
	}

	public void GetScrollViewFillerValues(float yScrollPos, int totalElementCount, float averageItemHeight, int maxVisibleCount, out int firstVisibleIndex, out int lastVisibleIndex, out float beginningFiller, out float endingFiller)
	{
		int num = Mathf.Min(totalElementCount, maxVisibleCount);
		firstVisibleIndex = Mathf.FloorToInt(yScrollPos / averageItemHeight);
		if (firstVisibleIndex > totalElementCount - num)
		{
			firstVisibleIndex = totalElementCount - num;
		}
		lastVisibleIndex = firstVisibleIndex + num;
		if (lastVisibleIndex >= totalElementCount)
		{
			lastVisibleIndex = totalElementCount - 1;
		}
		beginningFiller = (float)firstVisibleIndex * averageItemHeight;
		endingFiller = (float)(totalElementCount - lastVisibleIndex) * averageItemHeight;
	}

	private void OnDestroy()
	{
		this.WishSelectedHandlers = null;
	}
}
