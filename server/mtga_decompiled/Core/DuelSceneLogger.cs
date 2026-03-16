using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreClient.Rules;
using Newtonsoft.Json.Linq;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class DuelSceneLogger : IDisposable, IUpdate
{
	private struct EmoteUsedData
	{
		public string Key { get; private set; }

		public string Category { get; private set; }

		public uint Count { get; private set; }

		public EmoteUsedData(string key, string category, uint count)
		{
			Key = key;
			Category = category;
			Count = count;
		}
	}

	private struct MulliganedCardData
	{
		public uint GrpId { get; private set; }

		public string Name { get; private set; }

		public MulliganedCardData(uint grpId, string name)
		{
			GrpId = grpId;
			Name = name;
		}
	}

	private const int _passPriorityResponseThreshold = 250;

	private MatchManager _matchManager;

	private IBILogger _biLogger;

	private NPEState _npeState;

	private int _maxCreatures;

	private int _maxLands;

	private int _maxArtifactsAndEnchantments;

	private int _receivedPriorityCount;

	private int _passedPriorityCount;

	private int _spellsCastWithAutoPayCount;

	private int _spellsCastWithManualManaCount;

	private int _spellsCastWithMixedPayManaCount;

	private int _clickBpPetCount;

	private int _changeBpPetColorCount;

	private int _clickOpponentsBpPetCount;

	private DateTime _priorityReceivedUtc;

	private readonly List<TimeSpan> _timeSpansPassingPriority = new List<TimeSpan>();

	private readonly IDictionary<uint, int> _abilityUseByGrpId = new Dictionary<uint, int>();

	private readonly IDictionary<KeyValuePair<Phase, Step>, int> _actionsByLocalPhaseStep = new Dictionary<KeyValuePair<Phase, Step>, int>();

	private readonly IDictionary<KeyValuePair<Phase, Step>, int> _actionsByOpponentPhaseStep = new Dictionary<KeyValuePair<Phase, Step>, int>();

	private readonly IDictionary<string, int> _workflowCountByType = new Dictionary<string, int>();

	private uint _localPlayersTurnCount;

	private uint _opponentsTurnCount;

	private bool _matchEndMessageSent;

	private IDictionary<string, EmoteUsedData> _emotesUsed = new Dictionary<string, EmoteUsedData>();

	private int _muteCount;

	private int _unmuteCount;

	private List<uint> _startingHandCards = new List<uint>();

	private IList<ICollection<MulliganedCardData>> _mulliganedHands;

	private SettingsMessage _prevSettingsMessage;

	private DateTime _gameStartTimestamp;

	private DateTime _fullControlTimestamp;

	private uint _latestTurnWhereFullControlEnabled;

	private uint _secondsInFullControl;

	private MtgTimer _timerActivePlayer;

	private MtgTimer _timerNonactivePlayer;

	private float _clientElapsedActivePlayer;

	private float _clientElapsedNonactivePlayer;

	private uint _ropeShownCount;

	private bool _ropeShownActivePlayer;

	private bool _ropeShownNonactivePlayer;

	private uint _ropeExpiredCount;

	private uint _latestTurnWhereEdictIssued;

	private uint _startingTeamId;

	private bool _disposed;

	public GameManager GameManager { get; set; }

	private string GetStrigifiedAverageActionsByLocalPhaseStep()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<KeyValuePair<Phase, Step>, int> item in _actionsByLocalPhaseStep)
		{
			stringBuilder.AppendFormat("{0}, {1} : {2} ", item.Key.Key, item.Key.Value, (float)item.Value / (float)_localPlayersTurnCount);
		}
		return stringBuilder.ToString();
	}

	private string GetStrigifiedAverageActionsByOpponentPhaseStep()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<KeyValuePair<Phase, Step>, int> item in _actionsByOpponentPhaseStep)
		{
			stringBuilder.AppendFormat("{0}, {1} : {2}", item.Key.Key, item.Key.Value, (float)item.Value / (float)_opponentsTurnCount);
		}
		return stringBuilder.ToString();
	}

	private string StringifyAbilityUseByGrpId()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<uint, int> item in _abilityUseByGrpId)
		{
			stringBuilder.AppendFormat("GrpId: {0}, Count: {1}", item.Key, item.Value);
			stringBuilder.Append(Environment.NewLine);
		}
		return stringBuilder.ToString();
	}

	private JArray InteractionsToJObject()
	{
		JArray jArray = new JArray();
		foreach (KeyValuePair<string, int> item in _workflowCountByType)
		{
			jArray.Add($"{item.Key} : {item.Value.ToString()}");
		}
		return jArray;
	}

	public DuelSceneLogger(MatchManager mm, IBILogger biLogger, NPEState npeState)
	{
		_npeState = npeState;
		_biLogger = biLogger;
		_matchManager = mm;
		_matchManager.MatchStateChanged += OnMatchStateChanged;
		_mulliganedHands = new List<ICollection<MulliganedCardData>>();
	}

	public void Dispose()
	{
		OnDispose(manualDisposal: true);
		_disposed = true;
		GC.SuppressFinalize(this);
	}

	private void OnDispose(bool manualDisposal)
	{
		if (!_disposed && manualDisposal)
		{
			_mulliganedHands.Clear();
			GameManager = null;
			if (_matchManager != null)
			{
				_matchManager.MatchStateChanged -= OnMatchStateChanged;
				_matchManager.MatchCompleted -= OnMatchCompleted;
				_matchManager = null;
			}
		}
	}

	~DuelSceneLogger()
	{
		OnDispose(manualDisposal: false);
	}

	public void OnMulligan()
	{
		List<MtgCardInstance> visibleCards = GameManager.LatestGameState.LocalHand.VisibleCards;
		List<MulliganedCardData> list = new List<MulliganedCardData>(((ICollection<MtgCardInstance>)visibleCards).Count);
		foreach (MtgCardInstance item in (IEnumerable<MtgCardInstance>)visibleCards)
		{
			uint baseGrpId = item.BaseGrpId;
			string localizedText = GameManager.CardDatabase.GreLocProvider.GetLocalizedText(item.TitleId, "en-US", formatted: false);
			list.Add(new MulliganedCardData(baseGrpId, localizedText));
		}
		_mulliganedHands.Add(list);
	}

	public void UpdateStartingHand(List<uint> startingHandIds)
	{
		_startingHandCards = startingHandIds;
	}

	public void UpdateMaxCreatures(int count)
	{
		if (count > _maxCreatures)
		{
			_maxCreatures = count;
		}
	}

	public void UpdateMaxLands(int count)
	{
		if (count > _maxLands)
		{
			_maxLands = count;
		}
	}

	public void UpdateMaxArtifactsAndEnchantments(int count)
	{
		if (count > _maxArtifactsAndEnchantments)
		{
			_maxArtifactsAndEnchantments = count;
		}
	}

	public void PriorityReceived()
	{
		_priorityReceivedUtc = DateTime.UtcNow;
		_receivedPriorityCount++;
	}

	public void PriorityPassed()
	{
		TimeSpan item = DateTime.UtcNow - _priorityReceivedUtc;
		if (item.TotalMilliseconds >= 250.0)
		{
			_timeSpansPassingPriority.Add(item);
			_passedPriorityCount++;
		}
	}

	private float CalculateAveragePassPriorityWaitTime()
	{
		double num = 0.0;
		foreach (TimeSpan item in _timeSpansPassingPriority)
		{
			num += item.TotalSeconds;
		}
		return (float)((_timeSpansPassingPriority.Count > 0) ? (num / (double)_timeSpansPassingPriority.Count) : 0.0);
	}

	public void AbilityUsed(uint grpId)
	{
		if (!_abilityUseByGrpId.ContainsKey(grpId))
		{
			_abilityUseByGrpId[grpId] = 0;
		}
		_abilityUseByGrpId[grpId]++;
	}

	public void OnAccessoryInteraction(GREPlayerNum playerNum, AccessoryController.AccessoryInteraction interaction)
	{
		switch (playerNum)
		{
		case GREPlayerNum.LocalPlayer:
			switch (interaction)
			{
			case AccessoryController.AccessoryInteraction.Fidget:
				_clickBpPetCount++;
				break;
			case AccessoryController.AccessoryInteraction.ColorChange:
				_changeBpPetColorCount++;
				break;
			}
			break;
		case GREPlayerNum.Opponent:
			if (interaction == AccessoryController.AccessoryInteraction.Fidget || interaction == AccessoryController.AccessoryInteraction.ColorChange)
			{
				_clickOpponentsBpPetCount++;
			}
			break;
		}
	}

	public void UpdateActionsPerPhaseStep(Phase phase, Step step)
	{
		KeyValuePair<Phase, Step> key = new KeyValuePair<Phase, Step>(phase, step);
		if (GameManager.LatestGameState.ActivePlayer.IsLocalPlayer)
		{
			if (!_actionsByLocalPhaseStep.ContainsKey(key))
			{
				_actionsByLocalPhaseStep[key] = 0;
			}
			_actionsByLocalPhaseStep[key]++;
		}
		else
		{
			if (!_actionsByOpponentPhaseStep.ContainsKey(key))
			{
				_actionsByOpponentPhaseStep[key] = 0;
			}
			_actionsByOpponentPhaseStep[key]++;
		}
	}

	public void OnInteractionApplied(WorkflowBase workflowBase)
	{
		string key = workflowBase.ToString();
		if (!_workflowCountByType.ContainsKey(key))
		{
			_workflowCountByType[key] = 0;
		}
		_workflowCountByType[key]++;
	}

	public void EmoteUsed(string term, string category)
	{
		if (!_emotesUsed.ContainsKey(term))
		{
			_emotesUsed.Add(term, new EmoteUsedData(term, category, 0u));
		}
		_emotesUsed[term] = new EmoteUsedData(term, category, _emotesUsed[term].Count + 1);
	}

	public void OpponentMutedChanged(bool muted)
	{
		if (muted)
		{
			_muteCount++;
		}
		else
		{
			_unmuteCount++;
		}
	}

	public void OnClientUpdate(ClientUpdateBase clientUpdate)
	{
		BaseUserRequest interaction;
		if (clientUpdate is GreClient.Rules.GameStateUpdate)
		{
			GreClient.Rules.GameStateUpdate gameStateUpdate = clientUpdate as GreClient.Rules.GameStateUpdate;
			MtgGameState newState = gameStateUpdate.NewState;
			MtgGameState oldState = gameStateUpdate.OldState;
			uint num = 0u;
			uint num2 = 0u;
			foreach (GameRulesEvent change in gameStateUpdate.Changes)
			{
				if (change is TurnChangeEvent turnChangeEvent)
				{
					switch (turnChangeEvent.ActivePlayer.ClientPlayerEnum)
					{
					case GREPlayerNum.LocalPlayer:
						_localPlayersTurnCount++;
						break;
					case GREPlayerNum.Opponent:
						_opponentsTurnCount++;
						break;
					}
					if (newState.GameWideTurn > 1 && oldState != null && oldState.ActivePlayer != null)
					{
						OnTurnStop();
					}
					OnTurnStart();
				}
				else if (change is ManaPaidEvent manaPaidEvent && oldState != null)
				{
					uint manaId = manaPaidEvent.Mana.ManaId;
					if (oldState.LocalPlayer.IdToManaMap.ContainsKey(manaId))
					{
						num++;
					}
					else if (!oldState.Opponent.IdToManaMap.ContainsKey(manaId))
					{
						num2++;
					}
				}
			}
			if (num != 0 || num2 != 0)
			{
				if (num2 == 0)
				{
					_spellsCastWithManualManaCount++;
				}
				else if (num == 0)
				{
					_spellsCastWithAutoPayCount++;
				}
				else
				{
					_spellsCastWithMixedPayManaCount++;
				}
			}
			if (gameStateUpdate.NewState.DecidingPlayer != null && gameStateUpdate.NewState.DecidingPlayer.IsLocalPlayer)
			{
				_clientElapsedActivePlayer = 0f;
				_clientElapsedNonactivePlayer = 0f;
				foreach (MtgTimer timer in gameStateUpdate.NewState.DecidingPlayer.Timers)
				{
					switch (timer.TimerType)
					{
					case TimerType.ActivePlayer:
						_timerActivePlayer = timer;
						break;
					case TimerType.NonActivePlayer:
						_timerNonactivePlayer = timer;
						break;
					}
				}
			}
			if (newState.GameWideTurn == 1 && _startingTeamId == 0)
			{
				bool flag = newState.ActivePlayer?.IsLocalPlayer ?? false;
				uint num3 = _matchManager?.LocalPlayerSeatId ?? 2;
				_startingTeamId = (flag ? num3 : ((num3 != 1) ? 1u : 2u));
			}
			interaction = gameStateUpdate.Interaction;
			if (interaction is SubmitDeckRequest)
			{
				OnSideboardingCommenced();
				BaseUserRequest baseUserRequest = interaction;
				baseUserRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(baseUserRequest.OnSubmit, new Action<ClientToGREMessage>(onSubmit));
			}
		}
		else if (clientUpdate is SettingsNotification)
		{
			SettingsMessage settings = (clientUpdate as SettingsNotification).Settings;
			if (_prevSettingsMessage != null)
			{
				if (!_prevSettingsMessage.FullControlEnabled() && settings.FullControlEnabled())
				{
					OnFullControlEnabled();
				}
				else if (_prevSettingsMessage.FullControlEnabled() && !settings.FullControlEnabled())
				{
					OnFullControlDisabled();
				}
			}
			_prevSettingsMessage = settings;
		}
		else
		{
			if (!(clientUpdate is EdictalUpdate { Message: var message }))
			{
				return;
			}
			if (message != null)
			{
				ClientToGREMessage edictMessage = message.EdictMessage;
				if (edictMessage != null && edictMessage.Type == ClientMessageType.SubmitDeckResp)
				{
					OnSideboardingCompleted();
				}
			}
			OnEdictIssued();
		}
		void onSubmit(ClientToGREMessage msg)
		{
			OnSideboardingCompleted();
			BaseUserRequest baseUserRequest2 = interaction;
			baseUserRequest2.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(baseUserRequest2.OnSubmit, new Action<ClientToGREMessage>(onSubmit));
		}
	}

	public void OnUpdate(float dt)
	{
		if (_timerActivePlayer != null && _timerActivePlayer.Running)
		{
			_clientElapsedActivePlayer += dt;
			float num = _timerActivePlayer.RemainingTime - _clientElapsedActivePlayer;
			if (!_ropeShownActivePlayer && num <= (float)_timerActivePlayer.WarningThreshold)
			{
				_ropeShownCount++;
				_ropeShownActivePlayer = true;
			}
		}
		if (_timerNonactivePlayer != null && _timerNonactivePlayer.Running)
		{
			_clientElapsedNonactivePlayer += dt;
			float num2 = _timerNonactivePlayer.RemainingTime - _clientElapsedNonactivePlayer;
			if (!_ropeShownNonactivePlayer && num2 <= (float)_timerNonactivePlayer.WarningThreshold)
			{
				_ropeShownCount++;
				_ropeShownNonactivePlayer = true;
			}
		}
	}

	public void ModalSelectionError(uint abilityGrpId, uint grpId, string title, string error)
	{
		DuelSceneModalSelectionError payload = new DuelSceneModalSelectionError
		{
			EventTime = DateTime.UtcNow,
			AbilityGrpId = abilityGrpId,
			GrpId = grpId,
			Title = title,
			Error = error
		};
		_biLogger.Send(ClientBusinessEventType.DuelSceneModalSelectionError, payload);
	}

	private void OnMatchStateChanged(MatchState matchState)
	{
		switch (matchState)
		{
		case MatchState.GameInProgress:
			OnGameCommenced();
			break;
		case MatchState.GameComplete:
			OnGameCompleted();
			break;
		case MatchState.MatchComplete:
			OnMatchCompleted();
			break;
		}
	}

	private void OnGameCommenced()
	{
		_mulliganedHands.Clear();
		_gameStartTimestamp = default(DateTime);
		_fullControlTimestamp = default(DateTime);
		_secondsInFullControl = 0u;
		_latestTurnWhereFullControlEnabled = 0u;
		_timerActivePlayer = null;
		_timerNonactivePlayer = null;
		_clientElapsedActivePlayer = 0f;
		_clientElapsedNonactivePlayer = 0f;
		_ropeShownCount = 0u;
		_ropeShownActivePlayer = false;
		_ropeShownNonactivePlayer = false;
		_ropeExpiredCount = 0u;
		_latestTurnWhereEdictIssued = 0u;
		DuelSceneGameStart payload = new DuelSceneGameStart
		{
			SeatId = _matchManager.LocalPlayerSeatId,
			TeamId = _matchManager.LocalPlayerSeatId,
			GameNumber = _matchManager.CurrentGameNumber,
			MatchId = _matchManager.GreConnection.MatchId,
			EventId = GetEventId(),
			EventTime = DateTime.UtcNow,
			ChallengeId = (_matchManager.ChallengeData?.ChallengeId.ToString() ?? "")
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.DuelSceneGameStart, payload);
		_gameStartTimestamp = DateTime.UtcNow;
	}

	private void OnTurnStart()
	{
		if (GameManager.AutoRespManager.FullControlEnabled)
		{
			_latestTurnWhereFullControlEnabled = GameManager.LatestGameState.GameWideTurn;
		}
	}

	private void OnTurnStop()
	{
		_timerActivePlayer = null;
		_timerNonactivePlayer = null;
		_ropeShownActivePlayer = false;
		_ropeShownNonactivePlayer = false;
	}

	private void OnGameCompleted()
	{
		TimeSpan timeSpan = DateTime.UtcNow - _gameStartTimestamp;
		MatchManager.GameResult gameResult = _matchManager.GameResults[_matchManager.GameResults.Count - 1];
		JArray jArray = new JArray();
		foreach (ICollection<MulliganedCardData> mulliganedHand in _mulliganedHands)
		{
			JArray jArray2 = new JArray();
			foreach (MulliganedCardData item in mulliganedHand)
			{
				JObject jObject = new JObject();
				jObject["grpId"] = item.GrpId;
				jObject["name"] = item.Name;
				jArray2.Add(jObject);
			}
			jArray.Add(jArray2);
		}
		List<List<CardMulliganedData>> list = new List<List<CardMulliganedData>>();
		foreach (ICollection<MulliganedCardData> mulliganedHand2 in _mulliganedHands)
		{
			List<CardMulliganedData> list2 = new List<CardMulliganedData>();
			foreach (MulliganedCardData item2 in mulliganedHand2)
			{
				list2.Add(new CardMulliganedData(item2.GrpId, item2.Name));
			}
			list.Add(list2);
		}
		DuelSceneGameStop payload = new DuelSceneGameStop
		{
			SeatId = _matchManager.LocalPlayerSeatId,
			TeamId = _matchManager.LocalPlayerSeatId,
			GameNumber = _matchManager.CurrentGameNumber,
			MatchId = _matchManager.GreConnection.MatchId,
			EventId = GetEventId(),
			StartingTeamId = _startingTeamId,
			WinningTeamId = ((gameResult.Winner == GREPlayerNum.LocalPlayer) ? _matchManager.LocalPlayerSeatId : ((gameResult.Winner == GREPlayerNum.Opponent) ? ((_matchManager.LocalPlayerSeatId != 1) ? 1u : 2u) : 0u)),
			WinningType = gameResult.Result.ToString(),
			WinningReason = gameResult.Reason.ToString(),
			MulliganedHands = list,
			StartingHand = _startingHandCards,
			TurnCount = (GameManager ? GameManager.LatestGameState.GameWideTurn : 0u),
			SecondsCount = (uint)timeSpan.TotalSeconds,
			SecondsCountInFullControl = _secondsInFullControl,
			RopeShownCount = _ropeShownCount,
			RopeExpiredCount = _ropeExpiredCount,
			ClickBpPetCount = _clickBpPetCount,
			ChangeBpPetColorCount = _changeBpPetColorCount,
			ClickOpponentsBpPetCount = _clickOpponentsBpPetCount,
			EventTime = DateTime.UtcNow
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.DuelSceneGameStop, payload);
	}

	private void OnSideboardingCommenced()
	{
		DuelSceneSideboardStart payload = new DuelSceneSideboardStart
		{
			GameNumber = _matchManager.CurrentGameNumber,
			MatchId = _matchManager.GreConnection.MatchId,
			EventId = GetEventId(),
			EventTime = DateTime.UtcNow
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.DuelSceneSideboardStart, payload);
	}

	private void OnSideboardingCompleted()
	{
		DuelSceneSideboardStop payload = new DuelSceneSideboardStop
		{
			GameNumber = _matchManager.CurrentGameNumber,
			MatchId = _matchManager.GreConnection.MatchId,
			EventId = GetEventId(),
			EventTime = DateTime.UtcNow
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.DuelSceneSideboardStop, payload);
	}

	private void OnMatchCompleted()
	{
		if (_matchEndMessageSent)
		{
			return;
		}
		_timeSpansPassingPriority.Sort();
		DuelSceneEndOfMatchReport payload = new DuelSceneEndOfMatchReport
		{
			MatchId = _matchManager.GreConnection.MatchId,
			MaxCreatures = _maxCreatures.ToString(),
			MaxLands = _maxLands.ToString(),
			MaxArtifactsAndEnchantments = _maxArtifactsAndEnchantments.ToString(),
			LongestPassPriorityWaitTimeInSeconds = _timeSpansPassingPriority.LastOrDefault().ToString(),
			ShortestPassPriorityWaitTimeInSeconds = _timeSpansPassingPriority.FirstOrDefault().ToString(),
			AveragePassPriorityWaitTimeInSeconds = CalculateAveragePassPriorityWaitTime().ToString(),
			ReceivedPriorityCount = _receivedPriorityCount.ToString(),
			PassedPriorityCount = _passedPriorityCount.ToString(),
			RespondedToPriorityCount = (_receivedPriorityCount - _passedPriorityCount).ToString(),
			SpellsCastWithAutoPayCount = _spellsCastWithAutoPayCount.ToString(),
			SpellsCastWithManualManaCount = _spellsCastWithManualManaCount.ToString(),
			SpellsCastWithMixedPayManaCount = _spellsCastWithMixedPayManaCount.ToString(),
			AbilityUseByGrpId = StringifyAbilityUseByGrpId(),
			AverageActionsByLocalPhaseStep = GetStrigifiedAverageActionsByLocalPhaseStep(),
			AverageActionsByOpponentPhaseStep = GetStrigifiedAverageActionsByOpponentPhaseStep(),
			InteractionCount = InteractionsToJObject().ToString(),
			EventTime = DateTime.UtcNow
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.DuelSceneEndOfMatchReport, payload);
		_matchEndMessageSent = true;
		if (_emotesUsed == null)
		{
			return;
		}
		List<EmoteUsageData> list = new List<EmoteUsageData>();
		foreach (KeyValuePair<string, EmoteUsedData> item in _emotesUsed)
		{
			list.Add(new EmoteUsageData(item.Key, item.Value.Category, item.Value.Count));
		}
		DuelSceneEmoteUsageReport payload2 = new DuelSceneEmoteUsageReport
		{
			MatchId = _matchManager.GreConnection.MatchId,
			Emotes = list,
			EventTime = DateTime.UtcNow,
			MuteCount = _muteCount,
			UnmuteCount = _unmuteCount
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.DuelSceneEmoteUsageReport, payload2);
	}

	private void OnFullControlEnabled()
	{
		if (GameManager.LatestGameState.GameWideTurn != _latestTurnWhereFullControlEnabled)
		{
			_latestTurnWhereFullControlEnabled = GameManager.LatestGameState.GameWideTurn;
		}
		_fullControlTimestamp = DateTime.UtcNow;
	}

	private void OnFullControlDisabled()
	{
		_secondsInFullControl += (uint)(DateTime.UtcNow - _fullControlTimestamp).TotalSeconds;
	}

	private void OnEdictIssued()
	{
		if (GameManager.LatestGameState.GameWideTurn != _latestTurnWhereEdictIssued)
		{
			_latestTurnWhereEdictIssued = GameManager.LatestGameState.GameWideTurn;
			_ropeExpiredCount++;
		}
	}

	private string GetEventId()
	{
		string text = _matchManager?.Event?.PlayerEvent?.EventUXInfo?.PublicEventName;
		if (text == null)
		{
			text = ((_npeState?.ActiveNPEGame != null) ? "NPE" : ((_matchManager?.ChallengeData == null) ? string.Empty : "DirectGame"));
		}
		return text;
	}
}
