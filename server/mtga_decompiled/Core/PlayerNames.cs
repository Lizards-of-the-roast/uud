using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.PlayerNameViews;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class PlayerNames : MonoBehaviour
{
	[SerializeField]
	private PlayerName _localPlayerName;

	[SerializeField]
	private PlayerName _opponentName;

	[SerializeField]
	private Transform _nameViewRoot;

	[SerializeField]
	private PlayerName _namePrefab;

	[SerializeField]
	private float _infoRotationDelay;

	[SerializeField]
	private List<PlayerName> _playerNamesList = new List<PlayerName>();

	[SerializeField]
	private Transform _localPlayerNameViewRoot;

	[SerializeField]
	private Transform _opponentNameViewRoot;

	private Dictionary<uint, Dictionary<uint, CardColorFlags>> _playerCommanderColorsByTitleId = new Dictionary<uint, Dictionary<uint, CardColorFlags>>();

	private Dictionary<uint, uint> _playerIdByCompanionTitleId = new Dictionary<uint, uint>();

	private Dictionary<uint, HighlightType> _highlights = new Dictionary<uint, HighlightType>();

	private GameManager _gameManager;

	private MatchManager _matchManager;

	private IGreLocProvider _localizationManager;

	private ICardDatabaseAdapter _cardDatabase;

	private NPEState _npeState;

	private float _timeOfLastInfoRotation;

	private IHighlightController _highlightController = NullHighlightController.Default;

	private IPlayerNameViewManager _playerNameViewManager;

	public PlayerName LocalPlayerName => _localPlayerName;

	public PlayerName OpponentPlayerName => _opponentName;

	public List<PlayerName> PlayerNamesList => _playerNamesList;

	public void Init(MatchManager matchManager, GameManager gameManager, ICardDatabaseAdapter cardDatabase, NPEState npeState, IPlayerNameViewManager playerNameViewManager)
	{
		_gameManager = gameManager;
		_matchManager = matchManager;
		_cardDatabase = cardDatabase;
		_npeState = npeState;
		_localizationManager = _cardDatabase.GreLocProvider;
		_playerNameViewManager = playerNameViewManager;
		UpdatePlayersNames();
		if (!PlatformUtils.IsHandheld())
		{
			foreach (PlayerNameViewData allPlayerNameData in _playerNameViewManager.GetAllPlayerNameDataList())
			{
				if (allPlayerNameData.PlayerNum != GREPlayerNum.LocalPlayer)
				{
					PlayerDesignationWidget commanderWidget = allPlayerNameData.PlayerNameView.CommanderWidget;
					PlayerDesignationWidget secondCommanderWidget = allPlayerNameData.PlayerNameView.SecondCommanderWidget;
					commanderWidget.PointerEntered += OnOpponentCommanderPointerEnter;
					secondCommanderWidget.PointerEntered += OnOpponentSecondCommanderPointerEnter;
					commanderWidget.PointerExited += OnOpponentCommanderPointerExit;
					secondCommanderWidget.PointerExited += OnOpponentCommanderPointerExit;
					commanderWidget.Clicked += OnOpponentCommanderClick;
					secondCommanderWidget.Clicked += OnOpponentSecondCommanderClick;
					PlayerDesignationWidget companionWidget = allPlayerNameData.PlayerNameView.CompanionWidget;
					companionWidget.PointerEntered += OnOpponentCompanionPointerEnter;
					companionWidget.PointerExited += OnOpponentCompanionPointerExit;
					companionWidget.Clicked += OnOpponentCompanionClicked;
				}
			}
		}
		Languages.LanguageChangedSignal.Listeners += OnLocalizeEvent;
		_gameManager.UXEventQueue.EventExecutionCompleted += OnUxEventCompleted;
		_timeOfLastInfoRotation = Time.time;
	}

	public void SetHighlightController(IHighlightController highlightController)
	{
		_highlightController = highlightController ?? NullHighlightController.Default;
	}

	public void ActivateInfoItemsOnPlayerName(GREPlayerNum playerNum, bool enabled)
	{
		_playerNameViewManager.GetPlayerNameByGrePlayerNum(playerNum)?.ActivateInfoItems(enabled);
	}

	public void ConfigurePlayerNamesForHandheld16x9(Transform understackRoot)
	{
		foreach (PlayerNameViewData allPlayerNameData in _playerNameViewManager.GetAllPlayerNameDataList())
		{
			allPlayerNameData.PlayerNameView.TextRoot.SetParent(understackRoot, worldPositionStays: false);
			allPlayerNameData.PlayerNameView.TextRoot.gameObject.SetActive(value: false);
		}
	}

	private void InfoRotationEvent()
	{
		foreach (PlayerNameViewData allPlayerNameData in _playerNameViewManager.GetAllPlayerNameDataList())
		{
			allPlayerNameData.PlayerNameView.RotateInfoSlots();
		}
	}

	private void UpdatePlayersNames()
	{
		if (_npeState.ActiveNPEGame != null)
		{
			foreach (MatchManager.PlayerInfo player in _matchManager.Players)
			{
				_playerNameViewManager.CreatePlayerNameNPE(player.SeatId, _localPlayerNameViewRoot, _opponentNameViewRoot).InitializeInfoSlots();
			}
			return;
		}
		foreach (MatchManager.PlayerInfo player2 in _matchManager.Players)
		{
			_playerNameViewManager.CreatePlayerName(player2.SeatId, _localPlayerNameViewRoot, _opponentNameViewRoot).InitializeInfoSlots();
		}
	}

	public void SetPlayerCommanderInfo(uint playerId, uint titleId, CardColorFlags colorIdentity)
	{
		if (!_playerCommanderColorsByTitleId.TryGetValue(playerId, out var value) || !value.ContainsKey(titleId))
		{
			PlayerName playerNameById = _playerNameViewManager.GetPlayerNameById(playerId);
			if (_playerCommanderColorsByTitleId.ContainsKey(playerId))
			{
				value.Add(titleId, colorIdentity);
				SetCommanderText(playerNameById, value);
				return;
			}
			Dictionary<uint, CardColorFlags> dictionary = new Dictionary<uint, CardColorFlags> { { titleId, colorIdentity } };
			_playerCommanderColorsByTitleId.Add(playerId, dictionary);
			SetCommanderText(playerNameById, dictionary);
		}
	}

	private void OnLocalizeEvent()
	{
		foreach (PlayerNameViewData allPlayerNameData in _playerNameViewManager.GetAllPlayerNameDataList())
		{
			uint playerId = allPlayerNameData.PlayerId;
			if (_playerCommanderColorsByTitleId.TryGetValue(playerId, out var value))
			{
				SetCommanderText(allPlayerNameData.PlayerNameView, value);
			}
			if (_playerIdByCompanionTitleId.TryGetValue(playerId, out var value2))
			{
				SetCompanionText(allPlayerNameData.PlayerNameView, value2);
			}
		}
	}

	private void SetCommanderText(PlayerName nameView, Dictionary<uint, CardColorFlags> commanderColorsByTitleId)
	{
		string text = string.Empty;
		bool flag = true;
		foreach (KeyValuePair<uint, CardColorFlags> item in commanderColorsByTitleId)
		{
			uint key = item.Key;
			CardColorFlags value = item.Value;
			if (key != 0)
			{
				text = _localizationManager.GetLocalizedText(key);
				text = CardUtilities.FormatComplexTitle(text);
				IReadOnlyList<CardColorFlags> readOnlyList = value.ToDisplayOrder();
				if (readOnlyList.Count < 5)
				{
					string text2 = ManaUtilities.ConvertToOldSchoolManaText(ManaUtilities.ConvertCardColorFlagsToManaQuantities(readOnlyList));
					text2 = ManaUtilities.ConvertManaSymbols(text2);
					text = text + " " + text2;
				}
				nameView.CommanderWidget.GoldIdentityIconEnabled = readOnlyList.Count >= 5;
			}
			if (flag)
			{
				nameView.SetCommanderText(text);
				flag = false;
			}
			else
			{
				nameView.SetSecondCommanderText(text);
			}
			nameView.InitializeInfoSlots();
		}
	}

	private void OnOpponentCommanderClick()
	{
		ExamineCommanderCard(0);
	}

	private void OnOpponentSecondCommanderClick()
	{
		ExamineCommanderCard(1);
	}

	private void ExamineCommanderCard(int commanderIndex)
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		if (currentGameState == null)
		{
			return;
		}
		MtgPlayer opponent = currentGameState.Opponent;
		if (opponent == null)
		{
			return;
		}
		if (_gameManager.ViewManager.TryGetCardView(opponent.CommanderIds[commanderIndex], out var cardView))
		{
			_gameManager.CardHolderManager.Examine.ExamineCard(cardView.VisualModel);
			return;
		}
		DesignationData item = opponent.Designations.Find((DesignationData x) => x.Type == Designation.Commander);
		if (item.GrpId != 0)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item.GrpId);
			MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance();
			mtgCardInstance.Visibility = Visibility.None;
			mtgCardInstance.Owner = opponent;
			mtgCardInstance.Designations.Add(item);
			CardData model = new CardData(mtgCardInstance, cardPrintingById);
			_gameManager.CardHolderManager.Examine.ExamineCard(model);
		}
		else
		{
			Debug.LogError("No commander card instance found");
		}
	}

	private void OnOpponentCommanderPointerEnter()
	{
		HighlightCommanderCard(0);
	}

	private void OnOpponentSecondCommanderPointerEnter()
	{
		HighlightCommanderCard(1);
	}

	private void HighlightCommanderCard(int commanderIdIndex)
	{
		_highlights.Clear();
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		if (currentGameState != null)
		{
			MtgPlayer opponent = currentGameState.Opponent;
			if (opponent != null)
			{
				_highlights.Add(opponent.CommanderIds[commanderIdIndex], HighlightType.Selected);
			}
		}
		_highlightController.SetUserHighlights(_highlights);
	}

	private void OnOpponentCommanderPointerExit()
	{
		ClearHighlights();
	}

	public void SetPlayerCompanionTitleId(uint playerId, uint titleId)
	{
		PlayerName playerNameById = _playerNameViewManager.GetPlayerNameById(playerId);
		if (!_playerIdByCompanionTitleId.TryGetValue(playerId, out var value) || value != titleId)
		{
			if (_playerIdByCompanionTitleId.ContainsKey(playerId))
			{
				_playerIdByCompanionTitleId[playerId] = titleId;
			}
			else
			{
				_playerIdByCompanionTitleId.Add(playerId, titleId);
			}
			SetCompanionText(playerNameById, titleId);
			playerNameById.InitializeInfoSlots();
		}
	}

	private void SetCompanionText(PlayerName nameView, uint titleId)
	{
		string companionText = string.Empty;
		if (titleId != 0)
		{
			companionText = _localizationManager.GetLocalizedText(titleId);
			companionText = CardUtilities.FormatComplexTitle(companionText);
		}
		nameView.SetCompanionText(companionText);
		nameView.InitializeInfoSlots();
	}

	private void OnOpponentCompanionClicked()
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		if (currentGameState == null)
		{
			return;
		}
		MtgPlayer opponent = currentGameState.Opponent;
		if (opponent == null)
		{
			return;
		}
		if (_gameManager.ViewManager.TryGetCardView(opponent.CompanionId, out var cardView))
		{
			_gameManager.CardHolderManager.Examine.ExamineCard(cardView.VisualModel);
			return;
		}
		DesignationData item = opponent.Designations.Find((DesignationData x) => x.Type == Designation.Companion);
		if (item.GrpId != 0)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item.GrpId);
			MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance();
			mtgCardInstance.Visibility = Visibility.None;
			mtgCardInstance.Owner = opponent;
			mtgCardInstance.Designations.Add(item);
			CardData model = new CardData(mtgCardInstance, cardPrintingById);
			_gameManager.CardHolderManager.Examine.ExamineCard(model);
		}
		else
		{
			Debug.LogError("No commander card instance found");
		}
	}

	private void OnOpponentCompanionPointerEnter()
	{
		_highlights.Clear();
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		if (currentGameState != null)
		{
			MtgPlayer opponent = currentGameState.Opponent;
			if (opponent != null)
			{
				_highlights.Add(opponent.CompanionId, HighlightType.Selected);
			}
		}
		_highlightController.SetUserHighlights(_highlights);
	}

	private void OnOpponentCompanionPointerExit()
	{
		ClearHighlights();
	}

	private void ClearHighlights()
	{
		_highlights.Clear();
		_highlightController.SetUserHighlights(_highlights);
	}

	private void Update()
	{
		if (Time.time - _timeOfLastInfoRotation > _infoRotationDelay)
		{
			InfoRotationEvent();
			_timeOfLastInfoRotation = Time.time;
		}
	}

	private void OnDestroy()
	{
		if ((bool)_gameManager && _gameManager.UXEventQueue != null)
		{
			_gameManager.UXEventQueue.EventExecutionCompleted -= OnUxEventCompleted;
		}
		foreach (PlayerNameViewData allPlayerNameData in _playerNameViewManager.GetAllPlayerNameDataList())
		{
			if (allPlayerNameData.PlayerNum != GREPlayerNum.LocalPlayer)
			{
				PlayerDesignationWidget commanderWidget = allPlayerNameData.PlayerNameView.CommanderWidget;
				PlayerDesignationWidget secondCommanderWidget = allPlayerNameData.PlayerNameView.SecondCommanderWidget;
				if ((bool)commanderWidget)
				{
					commanderWidget.PointerEntered -= OnOpponentCommanderPointerEnter;
					secondCommanderWidget.PointerEntered -= OnOpponentSecondCommanderPointerEnter;
					commanderWidget.PointerExited -= OnOpponentCommanderPointerExit;
					secondCommanderWidget.PointerExited -= OnOpponentCommanderPointerExit;
					commanderWidget.Clicked -= OnOpponentCommanderClick;
					secondCommanderWidget.Clicked -= OnOpponentSecondCommanderClick;
				}
				PlayerDesignationWidget companionWidget = allPlayerNameData.PlayerNameView.CompanionWidget;
				if ((bool)companionWidget)
				{
					companionWidget.PointerEntered -= OnOpponentCompanionPointerEnter;
					companionWidget.PointerExited -= OnOpponentCompanionPointerExit;
					companionWidget.Clicked -= OnOpponentCompanionClicked;
				}
			}
		}
		Languages.LanguageChangedSignal.Listeners -= OnLocalizeEvent;
		_gameManager = null;
		_cardDatabase = null;
		_localizationManager = null;
		_highlightController = NullHighlightController.Default;
	}

	public void PlayIntroVFX()
	{
		PlayDesignationVFX(GREPlayerNum.LocalPlayer);
		PlayDesignationVFX(GREPlayerNum.Opponent);
	}

	private void PlayDesignationVFX(GREPlayerNum playerNum)
	{
		PlayerName playerNameByGrePlayerNum = _playerNameViewManager.GetPlayerNameByGrePlayerNum(playerNum);
		PlayWidgetVFX(playerNameByGrePlayerNum.CommanderWidget);
		PlayWidgetVFX(playerNameByGrePlayerNum.SecondCommanderWidget);
		PlayWidgetVFX(playerNameByGrePlayerNum.CompanionWidget);
	}

	private void PlayWidgetVFX(PlayerDesignationWidget widget)
	{
		if (widget.gameObject.activeSelf)
		{
			widget.PlayVFX();
		}
	}

	private uint GetGameWinsRequired()
	{
		uint result = 1u;
		if (_matchManager != null)
		{
			switch (_matchManager.WinCondition)
			{
			case MatchWinCondition.SingleElimination:
				result = 1u;
				break;
			case MatchWinCondition.Best2Of3:
				result = 2u;
				break;
			case MatchWinCondition.Best3Of5:
				result = 3u;
				break;
			}
		}
		return result;
	}

	private uint GetGameWins(GREPlayerNum playerNum)
	{
		uint num = 0u;
		foreach (MatchManager.GameResult gameResult in _matchManager.GameResults)
		{
			if (gameResult.Result == ResultType.WinLoss && gameResult.Winner == playerNum)
			{
				num++;
			}
		}
		return num;
	}

	private void OnUxEventCompleted(UXEvent uxEvent)
	{
		if (uxEvent is GameEndUXEvent { Loser: var loser })
		{
			switch (loser)
			{
			case GREPlayerNum.LocalPlayer:
				_playerNameViewManager.GetPlayerNameByGrePlayerNum(GREPlayerNum.Opponent).SetWins(GetGameWinsRequired(), GetGameWins(GREPlayerNum.Opponent));
				break;
			case GREPlayerNum.Opponent:
				_playerNameViewManager.GetPlayerNameByGrePlayerNum(GREPlayerNum.LocalPlayer).SetWins(GetGameWinsRequired(), GetGameWins(GREPlayerNum.LocalPlayer));
				break;
			}
		}
	}
}
