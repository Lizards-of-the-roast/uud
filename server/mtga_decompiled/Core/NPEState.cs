using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Core.NPEStitcher;
using Core.Shared.Code.DebugTools;
using GreClient.Network;
using Pooling;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.TcpConnection;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.BI;
using Wotc;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtgo.Gre.External.Messaging;

public class NPEState
{
	public enum TutorialStates
	{
		Unknown,
		EngageWithEvent,
		Underway,
		Completed
	}

	public enum NPEProgressMarker
	{
		Finished_Registration,
		Cinematic_Started,
		Cinematic_Finished,
		ENDOFPROLOGUE,
		NPE_Home_Hit_Onto_Game,
		Onto_Game,
		Started_Game,
		In_Game,
		Tutorial_Skipped_In_Game,
		Completed_Game,
		Tutorial_Skipped_After_Game,
		Onboarding_Skipped,
		NPE_Home_Monocolor_Deck_Reward_Shown,
		NPE_Home_Monocolor_Deck_Reward_Dismissed,
		Tutorial_Replayed
	}

	public struct NPEProgressContext
	{
		public NPEProgressMarker Checkpoint;

		public int? GameNumber;

		public uint TurnNumber;

		public Phase Phase;

		public Step Step;

		public bool Won;

		public bool FromStart;

		public NPEProgressContext(NPEProgressMarker checkpoint, int? gameNumber = null, uint turnNum = 0u, Phase phase = Phase.None, Step step = Step.None, bool won = false, bool fromStartUp = false)
		{
			Checkpoint = checkpoint;
			GameNumber = gameNumber;
			TurnNumber = turnNum;
			Phase = phase;
			Step = step;
			Won = won;
			FromStart = fromStartUp;
		}
	}

	public static List<NPE_Game> NPE_games = new List<NPE_Game>
	{
		new F1_NPEGame1_Combined(),
		new D2_NPEGame2_RedAggro(),
		new F3_NPEGame3_Auras(),
		new F4_NPEGame4_Assassin(),
		new G5_NPEGame5_6drop()
	};

	private HeadlessClient _npeClient;

	public System.Action InvokeNPEHomeSkipTutorialSequence;

	private IAccountClient _accountClient;

	private AccountInformation _accountInfo;

	private IBILogger _biLogger;

	private TutorialSkippedFromInGameSignal _tutorialSkippedFromInGameSignal;

	private ConnectionManager _connectionManager;

	public NPE_Game ActiveNPEGame { get; private set; }

	public int ActiveNPEGameNumber { get; private set; }

	public int LastJoinedNPEGameNumber { get; private set; }

	public TutorialStates TutorialState { get; private set; }

	public bool SkipTutorialButtonLocked { get; private set; }

	public bool FirstTutorialRunthrough { get; private set; }

	public bool SkipTutorialWasQueuedUpFromInGame { get; private set; }

	public IPreconDeckServiceWrapper PreconDeckManager { get; private set; }

	public static NPEState Create()
	{
		return new NPEState();
	}

	private NPEState()
	{
		TutorialState = TutorialStates.Unknown;
		FirstTutorialRunthrough = false;
		LastJoinedNPEGameNumber = -1;
		_accountClient = Pantry.Get<IAccountClient>();
		_accountInfo = _accountClient.AccountInformation;
		_biLogger = Pantry.Get<IBILogger>();
		PreconDeckManager = Pantry.Get<IPreconDeckServiceWrapper>();
		_connectionManager = Pantry.Get<ConnectionManager>();
		_tutorialSkippedFromInGameSignal = Pantry.Get<TutorialSkippedFromInGameSignal>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (System.Action)Delegate.Combine(connectionManager.OnFdReconnected, new System.Action(OnFdReconnect));
	}

	private void OnFdReconnect()
	{
		_accountInfo = _accountClient.AccountInformation;
	}

	public void SetStateToEngageTutorialEventFlow(bool thisIsFirstTimeDoingTutorial = false)
	{
		TutorialState = TutorialStates.EngageWithEvent;
		FirstTutorialRunthrough = thisIsFirstTimeDoingTutorial;
	}

	public void ConsiderTutorialCompleted()
	{
		TutorialState = TutorialStates.Completed;
		ActiveNPEGame = null;
	}

	public void NPEUnderway(int numberFromEvent)
	{
		TutorialState = TutorialStates.Underway;
		ActiveNPEGame = NPE_games[numberFromEvent];
		ActiveNPEGameNumber = numberFromEvent;
		ActiveNPEGame.Initialize();
	}

	public void RememberPlayingThisGame()
	{
		LastJoinedNPEGameNumber = ActiveNPEGameNumber;
		int activeNPEGameNumber = ActiveNPEGameNumber;
		MDNPlayerPrefs.UpdateNPEGameAttemptNumber(_accountInfo.PersonaID, activeNPEGameNumber);
	}

	public void LockDownSkipButtonWhileQueuing()
	{
		SkipTutorialButtonLocked = true;
	}

	public void UnlockSkipTutorialButton()
	{
		SkipTutorialButtonLocked = false;
	}

	public HeadlessClient NewNPEOpponent(IObjectPool objectPool, ICardDatabaseAdapter cardDatabase, int npeGameNumber)
	{
		_npeClient?.Dispose();
		BotTool botTool = Pantry.Get<BotTool>();
		DeckHeuristic aiConfig = botTool.DeckHeuristic ?? ScriptableObject.CreateInstance<DeckHeuristic>();
		botTool.SetAttackConfig(new AttackConfig(botTool.AttackConfig, uint.MaxValue));
		botTool.SetBlockConfig(new BlockConfig(botTool.BlockConfig, uint.MaxValue));
		IHeadlessClientStrategy strategy = new RequestHandlerStrategy(NPEStrategy.CreateHandlers(npeGameNumber, objectPool, new System.Random(), ActiveNPEGame, aiConfig, cardDatabase));
		Wizards.Arena.Client.Logging.ILogger logger = new ConsoleLogger();
		TcpConnection tcpConnection = new TcpConnection(logger, 17, ServicePointManager.ServerCertificateValidationCallback);
		IGREConnection greConnection = new GREConnection(Translate.ToConnectMessage(cardDatabase.VersionProvider.DataVersion, 2u), new LoggingConfig(), new MatchTcpConnection(tcpConnection), logger, ClientType.Familiar, RecordHistoryUtils.ShouldRecordHistory);
		return _npeClient = new HeadlessClient(greConnection, strategy, cardDatabase);
	}

	public void QueueUpSkipTutorialFromInGame()
	{
		SkipTutorialWasQueuedUpFromInGame = true;
		_tutorialSkippedFromInGameSignal.Dispatch(new SignalArgs(this));
		_npeClient.Concede(MatchScope.Game);
		BI_NPEProgressUpdate(new NPEProgressContext(NPEProgressMarker.Tutorial_Skipped_In_Game));
		BIEventTracker.TrackEvent(EBiEvent.CompletedNpe);
	}

	public void SkipTutorial(INpeStrategy provider)
	{
		SkipTutorialWasQueuedUpFromInGame = false;
		BI_NPEProgressUpdate(new NPEProgressContext(NPEProgressMarker.Tutorial_Skipped_After_Game));
		BIEventTracker.TrackEvent(EBiEvent.CompletedNpe);
		provider.SkipTutorial(delegate
		{
			ConsiderTutorialCompleted();
			InvokeNPEHomeSkipTutorialSequence?.Invoke();
		});
	}

	private static string DisplayStringForNPEProgress(NPEProgressContext checkpointContext)
	{
		NPEProgressMarker checkpoint = checkpointContext.Checkpoint;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		if (checkpoint < NPEProgressMarker.ENDOFPROLOGUE)
		{
			stringBuilder.Append((0.ToString() ?? "").PadLeft(2, '0'));
			stringBuilder.Append(" Prologue]");
		}
		else if (checkpointContext.GameNumber.HasValue)
		{
			stringBuilder.Append(((checkpointContext.GameNumber.Value + 1).ToString() ?? "").PadLeft(2, '0'));
			stringBuilder.Append(" Gameplay]");
		}
		else
		{
			stringBuilder.Append((99.ToString() ?? "").PadLeft(2, '0'));
			stringBuilder.Append(" Epilogue]");
		}
		int num = (int)checkpoint;
		stringBuilder.Append(" [" + (num.ToString() ?? "").PadLeft(2, '0') + " " + checkpoint);
		if (checkpointContext.GameNumber.HasValue)
		{
			stringBuilder.Append((checkpointContext.GameNumber.Value + 1).ToString() ?? "");
		}
		stringBuilder.Append("]");
		if (checkpointContext.Checkpoint == NPEProgressMarker.In_Game)
		{
			string[] obj = new string[7]
			{
				" [Turn ",
				(checkpointContext.TurnNumber.ToString() ?? "").PadLeft(2, '0'),
				"] [",
				null,
				null,
				null,
				null
			};
			num = (int)checkpointContext.Phase;
			obj[3] = (num.ToString() ?? "").PadLeft(2, '0');
			obj[4] = " ";
			obj[5] = checkpointContext.Phase.ToString();
			obj[6] = "]";
			stringBuilder.Append(string.Concat(obj));
			if (checkpointContext.Step != Step.None)
			{
				string[] obj2 = new string[5] { " [", null, null, null, null };
				num = (int)checkpointContext.Step;
				obj2[1] = (num.ToString() ?? "").PadLeft(2, '0');
				obj2[2] = " ";
				obj2[3] = checkpointContext.Step.ToString();
				obj2[4] = "]";
				stringBuilder.Append(string.Concat(obj2));
			}
		}
		return stringBuilder.ToString();
	}

	public void BI_NPEProgressUpdate(NPEProgressContext npeCheckpoint)
	{
		BI_NPEProgressUpdate(npeCheckpoint, _biLogger, _accountInfo);
	}

	public static void BI_NPEProgressUpdate(NPEProgressContext npeCheckpoint, IBILogger biLogger, AccountInformation accountInfo)
	{
		NPEProgress nPEProgress = new NPEProgress
		{
			EventTime = DateTime.UtcNow,
			Checkpoint = DisplayStringForNPEProgress(npeCheckpoint),
			Outcome = "invalid",
			Entered = "invalid",
			Attempt = -1
		};
		if (npeCheckpoint.Checkpoint == NPEProgressMarker.Completed_Game)
		{
			nPEProgress.Outcome = (npeCheckpoint.Won ? "won" : "lost");
		}
		if (npeCheckpoint.Checkpoint == NPEProgressMarker.NPE_Home_Hit_Onto_Game)
		{
			nPEProgress.Entered = (npeCheckpoint.FromStart ? "from start up" : "direct");
		}
		if (npeCheckpoint.GameNumber.HasValue && npeCheckpoint.Checkpoint != NPEProgressMarker.NPE_Home_Hit_Onto_Game)
		{
			nPEProgress.Attempt = MDNPlayerPrefs.GetNPEGameAttemptNumber(accountInfo?.PersonaID, npeCheckpoint.GameNumber.Value);
		}
		else
		{
			nPEProgress.Attempt = 1;
		}
		biLogger.Send(ClientBusinessEventType.NPEProgress, nPEProgress);
	}
}
