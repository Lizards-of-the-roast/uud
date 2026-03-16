using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.Code.Utils.PlayerPrefsUtils;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

public static class MDNPlayerPrefs
{
	public static class Strings
	{
		public const string PrefsVersion = "PrefsVersion";

		public const string UseTimers = "LOGIN-UseTimers";

		public const string PreviousEnvironment = "LOGIN-PreviousEnvironment";

		public const string DEBUG_PRV_MATCHDOOR_HOST = "LOGIN-PrvMDHost";

		public const string DEBUG_PRV_MATCHDOOR_PORT = "LOGIN-PrvMDPort";

		public const string DEBUG_MATCH_CONFIG_SUB_DIRECTORY = "DEBUG_MatchConfigSubDirectory";

		public const string DEBUG_MATCH_CONFIG_NAME = "DEBUG_MatchConfigName";

		public const string DEBUG_AlwaysSurvey = "DEBUG_AlwaysSurvey";

		public const string DEBUG_MOCK_PLAYER_INBOX_SERVICE = "DEBUG_MockPlayerInboxService";

		public const string DEBUG_DAMAGE_ASSIGNMENT_MODE = "DEBUG_DAMAGE_ASSIGNMENT_MODE";

		public const string PLAYERPREFS_KEY_MUSICVOLUME = "PlayerPrefs_Audio_MusicVolume";

		public const string PLAYERPREFS_KEY_MASTERVOLUME = "PlayerPrefs_Audio_MasterVolume";

		public const string PLAYERPREFS_KEY_AMBIENCEVOLUME = "PlayerPrefs_Audio_AmbienceVolume";

		public const string PLAYERPREFS_KEY_SFXVOLUME = "PlayerPrefs_Audio_SFXVolume";

		public const string PLAYERPREFS_KEY_VOVOLUME = "PlayerPrefs_Audio_VOVolume";

		public const string PLAYERPREFS_KEY_BACKGROUNDAUDIO = "PlayerPrefs_Audio_Background";

		public const string ClientLanguage = "ClientLanguage";

		public const string InitialLanguage = "HasDoneInitalLanguageSelect";

		public const string Birthday = "Birthday";

		public const string Country = "Country";

		public const string Experience = "Experience";

		public const string WAS_RefreshToken = "WAS-RefreshToken";

		public const string WAS_RefreshTokenEncrypted = "WAS-RefreshTokenE";

		public const string DisableEmotes = "Settings_DisableEmotes";

		public const string AutoPayMana = "PlayerPrefs_AutoPayMana";

		public const string SelectedDeckId = "SelectedDeckId";

		public const string SelectedStandardPlayDeckId = "SelectedStandardPlayDeckId";

		public const string SelectedEvent = "SelectedEvent";

		public const string LastPlayQueueTileEvent = "LastPlayQueueTileEvent";

		public const string UseColumnView = "DeckBuilder.ColumnView";

		public const string LargeCardsInPool = "DeckBuilder.LargeCardsInPool";

		public const string DraftDeckSideboardIds = "Draft.DeckSideboardIds";

		public const string DraftHasSeenVaultPopup = "Draft.HasSeenVaultPopup";

		public const string HasSeenSideboardSubmitTip = "DeckBuilder.HasSeenSideboardSubmitTip";

		public const string HasSeenStore = "Store.HasUserEverUsedg";

		public const string StoreTermsandConditions = "Store.TermsandConditions";

		public const string StoreCurrencySelection = "Store.CurrencySelection";

		public const string StoreDecksViewed = "Store.DecksViewed";

		public const string VouchersViewed = "Store.VouchersViewed";

		public const string StoreViewed = "Store.Viewed";

		public const string SetMasteryViewed = "SetMastery.Viewed";

		public const string SetMasterySpendHeatViewed = "SetMastery.SpendHeat.Viewed";

		public const string SelectedAvatar = "Profile.SelectedAvatar";

		public const string SelectedSleeve = "Profile.SelectedSleeve";

		public const string FavoriteSleeve = "FavoriteSleeve";

		public const string SelectedAccessory = "Profile.SelectedAccessory";

		public const string SelectedAccessoryMods = "Profile.SelectedAccessoryMods";

		public const string HasOpenedSealedPool = "Event.HasOpenedSealedPool";

		public const string StateMachineFlags = "StateMachineFlags";

		public const string SetAnnouncementsViewed = "SetAnnouncementsViewed";

		public const string RuleChangeViewed = "RuleChangeViewed";

		public const string OnboardingSkipped = "OnboardingSkipped";

		public const string BannedCardsAcknowledgedForFormat = "BannedCardsAcknowledgedForFormat";

		public const string SuspendedCardsAcknowledgedForFormat = "SuspendedCardsAcknowledgedForFormat";

		public const string RestrictedCardsAcknowledgedForFormat = "RestrictedCardsAcknowledgedForFormat";

		public const string HasSeenHandheldMOZTutorial = "HasSeenHandheldMOZTutorial";

		public const string BoosterPackOpenAutoReveal = "BoosterPackOpenAutoReveal";

		public const string BoosterPackOpenSkipAnimation = "BoosterPackOpenSkipAnimation";

		public const string ShouldValidateServerCert = "CheckSC";

		public const string InactivityTimeoutMs = "InactivityTimeoutMs";

		public const string DoorbellOverrideContent = "DoorbellOverrideContent";

		public const string DoorbellOverrideToggle = "DoorbellOverrideToggle";

		public const string IsBackgroundDownloadEnabled = "IsBackgroundDownloadEnabled";

		public const string UseMomir = "UseMomir";

		public const string TextboxScrollSensitivity = "TextboxScrollSensitivity";

		public const string ShowEvergreenKeywordReminders = "ShowEvergreenKeywordReminders";

		public const string AutoOrderTriggers = "AutoOrderTriggers";

		public const string AutoAssignCombatDamage = "AutoAssignCombatDamage";

		public const string AutoChooseReplacementEffects = "AutoChooseReplacementEffects";

		public const string AutoApplyCardStyles = "AutoApplyCardStyles";

		public const string HideAltArtStyles = "HideAltArtStyles";

		public const string ShowPhaseLadder = "ShowPhaseLadder";

		public const string AllPlayModesToggle = "AllPlayModesToggle";

		public const string PlayerHasToggledForTheFirstTime = "PlayerHasToggledForTheFirstTime";

		public const string GameplayWarningsEnabled = "GameplayWarningsEnabled";

		public const string FixedRulesTextSize = "FixedRulesTextSize";

		public const string PrivateGameChallenge = "PrivateGameChallenge";

		public const string PrivateGameChallengesMigrated = "PrivateGameChallengesMigrated";

		public const string PrivateGameChallengeListLastIndex = "PrivateGameChallengeListIndex";

		public const string NPEGameAttempt_Game = "NPEGame";

		public const string NPEGameAttempt_Attempt = "Attempt";

		public const string LastLoginDate = "LastLoginDate";

		public const string LastLoginEmail = "LastLoginString";

		public const string LoggedOutReason = "LoggedOutReason";

		public const string InitializedVariable_AskMagicExperienceLevel = "InitializedVariable_AskMagicExperienceLevel";

		public const string AskMagicExperienceLevel = "AskMagicExperienceLevel";

		public const string DeeplinkURLForNextSession = "DeeplinkURLForNextSession";

		public const string Experiment001PreventSkipTutorial_InExperimentalGroup = "Ex001PST";

		public const string Experiment002pvpLockedUntilLevel3_InExperimentalGroup = "Ex002PLUL3";

		public const string Experiment003noStitcherBetweenG1AndG2_InExperimentalGroup = "Ex003NSBG1AG2";

		public const string HashFilesSkippedOnce = "HashFilesSkippedOnce";

		public const string HashFilesOnStartup = "HashFilesOnStartup";

		public const string FileToHashOnStartup = "FileToHashOnStartup";

		public const string HashedFilesLastStartup = "HashedFilesLastStartup";

		public const string UseVerboseLogs = "UseVerboseLogs";

		public const string HasEverForcedVerboseLogging = "HasEverForcedVerbose";

		public const string ShownQualifierBadge = "ShownQualifierBadge";

		public const string FirstTimeSleeveNotify = "FirstTimeSleeveNotify";

		public const string FirstTimeCosmeticNotify = "FirstTimeCosmeticNotify";

		public const string HasSeenSleeveNotify = "HasSeenSleeveNotify";

		public const string HasSeenCosmeticNotify = "HasSeenCosmeticNotify";

		public const string LocalPlayerPresence = "LocalPlayerPresence";

		public const string SupplementalInstallID = "SupplementalInstallID";

		public const string ReconnectBattlefieldId = "ReconnectBattlefieldId";

		public const string ReconnectCardbackId = "ReconnectCardbackId";

		public const string ReconnectOpponentCardbackId = "ReconnectOpponentCardbackId";

		public const string ReconnectPetName = "ReconnectPetName";

		public const string ReconnectPetVariant = "ReconnectPetVariant";

		public const string ReconnectOpponentPetName = "ReconnectOpponentPetName";

		public const string ReconnectOpponentPetVariant = "ReconnectOpponentPetVariant";

		public const string ReconnectEmotesList = "ReconnectEmotesList";

		public const string ReconnectOpponentEmotesList = "ReconnectOpponentEmotesList";

		public const string ReconnectTitle = "ReconnectTitle";

		public const string ReconnectOpponentTitle = "ReconnectOpponentTitle";

		public const string ReconnectLimitedRankInfo = "ReconnectLimitedRankInfo";

		public const string ReconnectConstructedRankInfo = "ReconnectConstructedRankInfo";

		public const string MuteSparky = "MuteSparky";

		public const string BundleSource = "BundleEndpoint";

		public const string SelectedBundleSourceHashCode = "SelectedBundleSourceHashCode";

		public const string RotationEducationViewed = "RotationEducationViewed";

		public const string DebugLocalEventPaths = "DebugLocalEventPaths";

		public const string DebugLocalCoursePaths = "DebugLocalCoursePaths";

		public const string DebugLocalCarouselPaths = "DebugLocalCarouselPaths";

		public const string AgeGateTime = "AgeGateTime";

		public const string DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError = "DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError";

		public const string DEBUG_AutoplayHighlightLogFontSize = "HighlightLogFontSize";

		public const string DEBUG_IncludeAllCardTestDeckFormats = "DEBUG_IncludeAllCardTestDeckFormats";

		public const string ShowReviewRequest = "ShowReviewRequest";

		public const string DebugForceUpdate = "Debug_ForceUpdate";

		public const string RunningDSS = "RunningDSS";

		public const string SaveDSReplay = "SaveDSReplay";

		public const string QueuedAutoplayFile = "QueuedAutoplayFile";

		public const string DSSReportName = "DSSReportName";

		public const string ReplayName = "ReplayName";

		public const string SparkRankRewardShown = "SparkRankRewardShown";

		public const string SparkRankReturnHome = "SparkRankReturnHome";

		public const string ExpandedDeckFolders = "ExpandedDeckFolders";

		public const string HasSeenPlayBlade = "HasSeenPlayBlade";

		public const string NPEObjectiveLastSeen = "NPEObjectiveLastSeen";

		public const string DisplayEnvironmentAndBundleEndpointSelectors = "DisplayEnvironmentAndBundleEndpointSelectors";
	}

	private const int PREFS_VERSION = 1;

	private static int _version = 1;

	private static IAccountClient _accountClient;

	private static string _cachedClientLanguage = null;

	public const int CHALLENGE_HISTORY_LENGTH = 5;

	private static IAccountClient AccountClient
	{
		get
		{
			if (_accountClient == null)
			{
				_accountClient = Pantry.Get<IAccountClient>();
				_accountClient.LoginStateChanged += OnAccountClientLoginStateChanged;
			}
			return _accountClient;
		}
	}

	private static string UserPersonaID => AccountClient?.AccountInformation?.PersonaID;

	public static string PreviousFDServer
	{
		get
		{
			try
			{
				if (CachedPlayerPrefs.HasKey("LOGIN-PreviousEnvironment"))
				{
					return CachedPlayerPrefs.GetString("LOGIN-PreviousEnvironment").Split('\n').FirstOrDefault((string s) => s.StartsWith(Application.dataPath))?.Split(',').Last();
				}
				return string.Empty;
			}
			catch
			{
				CachedPlayerPrefs.DeleteKey("LOGIN-PreviousEnvironment");
				return string.Empty;
			}
		}
		set
		{
			try
			{
				string text = CachedPlayerPrefs.GetString("LOGIN-PreviousEnvironment");
				if (string.IsNullOrWhiteSpace(text))
				{
					CachedPlayerPrefs.SetString("LOGIN-PreviousEnvironment", Application.dataPath + "," + value);
					CachedPlayerPrefs.Save();
					return;
				}
				string[] array = text.Split('\n');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].StartsWith(Application.dataPath))
					{
						array[i] = Application.dataPath + "," + value;
						CachedPlayerPrefs.SetString("LOGIN-PreviousEnvironment", string.Join("\n", array));
						CachedPlayerPrefs.Save();
						return;
					}
				}
				CachedPlayerPrefs.SetString("LOGIN-PreviousEnvironment", Application.dataPath + "," + value + "\n" + text);
				CachedPlayerPrefs.Save();
			}
			catch
			{
				CachedPlayerPrefs.SetString("LOGIN-PreviousEnvironment", Application.dataPath + "," + value);
				CachedPlayerPrefs.Save();
			}
		}
	}

	public static int DEBUG_Damage_Assignment_Mode
	{
		get
		{
			return PlayerPrefsExt.GetInt("DEBUG_DAMAGE_ASSIGNMENT_MODE");
		}
		set
		{
			PlayerPrefsExt.SetInt("LOGIN-PrvMDHost", value);
		}
	}

	public static string DEBUG_MatchDoorHost
	{
		get
		{
			return PlayerPrefsExt.GetString("LOGIN-PrvMDHost");
		}
		set
		{
			PlayerPrefsExt.SetString("LOGIN-PrvMDHost", value.ToString());
			CachedPlayerPrefs.Save();
		}
	}

	public static string DEBUG_MatchConfigSubDirectory
	{
		get
		{
			return PlayerPrefsExt.GetString("DEBUG_MatchConfigSubDirectory", string.Empty);
		}
		set
		{
			PlayerPrefsExt.SetString("DEBUG_MatchConfigSubDirectory", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string DEBUG_MatchConfigName
	{
		get
		{
			return PlayerPrefsExt.GetString("DEBUG_MatchConfigName");
		}
		set
		{
			PlayerPrefsExt.SetString("DEBUG_MatchConfigName", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static int DEBUG_MatchDoorPort
	{
		get
		{
			return PlayerPrefsExt.GetInt("LOGIN-PrvMDPort");
		}
		set
		{
			PlayerPrefsExt.SetInt("LOGIN-PrvMDPort", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static DateTime AgeGateTime
	{
		get
		{
			DateTime.TryParse(PlayerPrefsExt.GetString("AgeGateTime"), out var result);
			return result;
		}
		set
		{
			PlayerPrefsExt.SetString("AgeGateTime", value.ToString(CultureInfo.InvariantCulture));
			CachedPlayerPrefs.Save();
		}
	}

	public static bool ShowReviewRequest
	{
		get
		{
			return PlayerPrefsExt.GetBool("ShowReviewRequest", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("ShowReviewRequest", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string SupplementalInstallID
	{
		get
		{
			if (string.IsNullOrEmpty(CachedPlayerPrefs.GetString("SupplementalInstallID")))
			{
				CachedPlayerPrefs.SetString("SupplementalInstallID", "NoInstallID-" + Guid.NewGuid().ToString());
				CachedPlayerPrefs.Save();
			}
			return CachedPlayerPrefs.GetString("SupplementalInstallID");
		}
	}

	public static bool UseTimers
	{
		get
		{
			return PlayerPrefsExt.GetBool("LOGIN-UseTimers");
		}
		set
		{
			PlayerPrefsExt.SetBool("LOGIN-UseTimers", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool AutoPayMana
	{
		get
		{
			return PlayerPrefsExt.GetBool("PlayerPrefs_AutoPayMana", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("PlayerPrefs_AutoPayMana", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool DEBUG_AlwaysSurvey
	{
		get
		{
			return PlayerPrefsExt.GetBool("DEBUG_AlwaysSurvey", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("DEBUG_AlwaysSurvey", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool DEBUG_MockPlayerInboxService
	{
		get
		{
			return PlayerPrefsExt.GetBool("DEBUG_MockPlayerInboxService", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("DEBUG_MockPlayerInboxService", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool ShowEvergreenKeywordReminders
	{
		get
		{
			return PlayerPrefsExt.GetBool("ShowEvergreenKeywordReminders", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("ShowEvergreenKeywordReminders", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool AutoOrderTriggers
	{
		get
		{
			return PlayerPrefsExt.GetBool("AutoOrderTriggers", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("AutoOrderTriggers", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool AutoChooseReplacementEffects
	{
		get
		{
			return PlayerPrefsExt.GetBool("AutoChooseReplacementEffects", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("AutoChooseReplacementEffects", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool AutoApplyCardStyles
	{
		get
		{
			return GetUserBool(UserPersonaID, "AutoApplyCardStyles", defaultValue: true);
		}
		set
		{
			SetUserBool(UserPersonaID, "AutoApplyCardStyles", value);
		}
	}

	public static bool HideAltArtStyles
	{
		get
		{
			return GetUserBool(UserPersonaID, "HideAltArtStyles");
		}
		set
		{
			SetUserBool(UserPersonaID, "HideAltArtStyles", value);
		}
	}

	public static bool GameplayWarningsEnabled
	{
		get
		{
			return GetUserBool(UserPersonaID, "GameplayWarningsEnabled", defaultValue: true);
		}
		set
		{
			SetUserBool(UserPersonaID, "GameplayWarningsEnabled", value);
		}
	}

	public static bool ShowPhaseLadder
	{
		get
		{
			return (GetUserInt(UserPersonaID, "ShowPhaseLadder") & 1) != 0;
		}
		set
		{
			int userInt = GetUserInt(UserPersonaID, "ShowPhaseLadder");
			SetUserInt(UserPersonaID, "ShowPhaseLadder", value ? (userInt | 1) : (userInt & -2));
		}
	}

	public static bool SeenPhaseLadderHint
	{
		get
		{
			return (GetUserInt(UserPersonaID, "ShowPhaseLadder") & 2) != 0;
		}
		set
		{
			int userInt = GetUserInt(UserPersonaID, "ShowPhaseLadder");
			SetUserInt(UserPersonaID, "ShowPhaseLadder", value ? (userInt | 2) : (userInt & -3));
		}
	}

	public static bool FixedRulesTextSize
	{
		get
		{
			return GetUserBool(UserPersonaID, "FixedRulesTextSize");
		}
		set
		{
			SetUserBool(UserPersonaID, "FixedRulesTextSize", value);
		}
	}

	public static bool AllPlayModesToggle
	{
		get
		{
			return PlayerPrefsExt.GetBool("AllPlayModesToggle", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("AllPlayModesToggle", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool PlayerHasToggledForTheFirstTime
	{
		get
		{
			return PlayerPrefsExt.GetBool("PlayerHasToggledForTheFirstTime", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("PlayerHasToggledForTheFirstTime", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool FirstTimeSleeveNotify
	{
		get
		{
			return PlayerPrefsExt.GetBool("FirstTimeSleeveNotify", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("FirstTimeSleeveNotify", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool FirstTimeCosmeticNotify
	{
		get
		{
			return PlayerPrefsExt.GetBool("FirstTimeCosmeticNotify", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("FirstTimeCosmeticNotify", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool StraightToDuelScene
	{
		get
		{
			return PlayerPrefsExt.GetBool("StraightToDuelScene", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("StraightToDuelScene", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool RecordServerMessageHistory
	{
		get
		{
			return PlayerPrefsExt.GetBool("RecordServerMessageHistory", Application.isEditor);
		}
		set
		{
			PlayerPrefsExt.SetBool("RecordServerMessageHistory", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool HasSeenSleeveNotify
	{
		get
		{
			return PlayerPrefsExt.GetBool("HasSeenSleeveNotify", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("HasSeenSleeveNotify", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool HasSeenCosmeticNotify
	{
		get
		{
			return PlayerPrefsExt.GetBool("HasSeenCosmeticNotify", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("HasSeenCosmeticNotify", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static int LocalPlayerPresence
	{
		get
		{
			return PlayerPrefsExt.GetInt("LocalPlayerPresence", 3);
		}
		set
		{
			PlayerPrefsExt.SetInt("LocalPlayerPresence", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static float PLAYERPREFS_KEY_MUSICVOLUME
	{
		get
		{
			float result = 80f;
			if (CachedPlayerPrefs.HasKey("PlayerPrefs_Audio_MusicVolume"))
			{
				result = PlayerPrefsExt.GetFloat("PlayerPrefs_Audio_MusicVolume");
			}
			return result;
		}
		set
		{
			PlayerPrefsExt.SetFloat("PlayerPrefs_Audio_MusicVolume", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static float PLAYERPREFS_KEY_MASTERVOLUME
	{
		get
		{
			float result = 100f;
			if (CachedPlayerPrefs.HasKey("PlayerPrefs_Audio_MasterVolume"))
			{
				result = PlayerPrefsExt.GetFloat("PlayerPrefs_Audio_MasterVolume");
			}
			return result;
		}
		set
		{
			PlayerPrefsExt.SetFloat("PlayerPrefs_Audio_MasterVolume", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static float PLAYERPREFS_KEY_AMBIENCEVOLUME
	{
		get
		{
			float result = 100f;
			if (CachedPlayerPrefs.HasKey("PlayerPrefs_Audio_AmbienceVolume"))
			{
				result = PlayerPrefsExt.GetFloat("PlayerPrefs_Audio_AmbienceVolume");
			}
			return result;
		}
		set
		{
			PlayerPrefsExt.SetFloat("PlayerPrefs_Audio_AmbienceVolume", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static float PLAYERPREFS_KEY_SFXVOLUME
	{
		get
		{
			float result = 100f;
			if (CachedPlayerPrefs.HasKey("PlayerPrefs_Audio_SFXVolume"))
			{
				result = PlayerPrefsExt.GetFloat("PlayerPrefs_Audio_SFXVolume");
			}
			return result;
		}
		set
		{
			PlayerPrefsExt.SetFloat("PlayerPrefs_Audio_SFXVolume", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static float PLAYERPREFS_KEY_VOVOLUME
	{
		get
		{
			float result = 100f;
			if (CachedPlayerPrefs.HasKey("PlayerPrefs_Audio_VOVolume"))
			{
				result = PlayerPrefsExt.GetFloat("PlayerPrefs_Audio_VOVolume");
			}
			return result;
		}
		set
		{
			PlayerPrefsExt.SetFloat("PlayerPrefs_Audio_VOVolume", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool HashFilesSkippedOnce
	{
		get
		{
			if (CachedPlayerPrefs.HasKey("HashFilesSkippedOnce"))
			{
				return PlayerPrefsExt.GetBool("HashFilesSkippedOnce");
			}
			return false;
		}
		set
		{
			PlayerPrefsExt.SetBool("HashFilesSkippedOnce", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool HashedFilesLastStartup
	{
		get
		{
			if (CachedPlayerPrefs.HasKey("HashedFilesLastStartup"))
			{
				return PlayerPrefsExt.GetBool("HashedFilesLastStartup");
			}
			return false;
		}
		set
		{
			PlayerPrefsExt.SetBool("HashedFilesLastStartup", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string FileToHashOnStartup
	{
		get
		{
			return PlayerPrefsExt.GetString("FileToHashOnStartup", string.Empty);
		}
		set
		{
			PlayerPrefsExt.SetString("FileToHashOnStartup", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool HashAllFilesOnStartup
	{
		get
		{
			return PlayerPrefsExt.GetBool("HashFilesOnStartup", defaultValue: false);
		}
		set
		{
			if (value)
			{
				FileToHashOnStartup = string.Empty;
			}
			PlayerPrefsExt.SetBool("HashFilesOnStartup", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError
	{
		get
		{
			if (CachedPlayerPrefs.HasKey("DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError"))
			{
				return PlayerPrefsExt.GetBool("DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError");
			}
			return true;
		}
		set
		{
			PlayerPrefsExt.SetBool("DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool DEBUG_IncludeAllCardTestDeckFormats
	{
		get
		{
			if (CachedPlayerPrefs.HasKey("DEBUG_IncludeAllCardTestDeckFormats"))
			{
				return PlayerPrefsExt.GetBool("DEBUG_IncludeAllCardTestDeckFormats");
			}
			return false;
		}
		set
		{
			PlayerPrefsExt.SetBool("DEBUG_IncludeAllCardTestDeckFormats", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool PLAYERPREFS_KEY_BACKGROUNDAUDIO
	{
		get
		{
			if (CachedPlayerPrefs.HasKey("PlayerPrefs_Audio_Background"))
			{
				return PlayerPrefsExt.GetBool("PlayerPrefs_Audio_Background");
			}
			return false;
		}
		set
		{
			PlayerPrefsExt.SetBool("PlayerPrefs_Audio_Background", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool DisableEmotes
	{
		get
		{
			return PlayerPrefsExt.GetBool("Settings_DisableEmotes");
		}
		set
		{
			PlayerPrefsExt.SetBool("Settings_DisableEmotes", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static DateTime Accounts_DateLastLoggedIn
	{
		get
		{
			return Convert.ToDateTime(CachedPlayerPrefs.GetString("LastLoginDate", DateTime.Now.ToString()));
		}
		set
		{
			CachedPlayerPrefs.SetString("LastLoginDate", DateTime.Now.ToString());
			CachedPlayerPrefs.Save();
		}
	}

	public static TimeSpan Accounts_TimeSinceLastLogin => DateTime.Now.Subtract(Accounts_DateLastLoggedIn);

	public static string Accounts_LastLogin_Email
	{
		get
		{
			return CachedPlayerPrefs.GetString("LastLoginString", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("LastLoginString", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string Accounts_LoggedOutReason
	{
		get
		{
			return CachedPlayerPrefs.GetString("LoggedOutReason", "INVALID");
		}
		set
		{
			CachedPlayerPrefs.SetString("LoggedOutReason", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string PLAYERPREFS_ClientLanguage
	{
		get
		{
			if (_cachedClientLanguage == null)
			{
				if (CachedPlayerPrefs.HasKey("ClientLanguage"))
				{
					string text = CachedPlayerPrefs.GetString("ClientLanguage");
					if (Languages.ExternalLanguages.Contains(text))
					{
						_cachedClientLanguage = text;
						return _cachedClientLanguage;
					}
				}
				_cachedClientLanguage = "en-US";
			}
			return _cachedClientLanguage;
		}
		set
		{
			_cachedClientLanguage = value;
			CachedPlayerPrefs.SetString("ClientLanguage", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string DeeplinkURLForNextSession
	{
		get
		{
			return CachedPlayerPrefs.GetString("DeeplinkURLForNextSession");
		}
		set
		{
			CachedPlayerPrefs.SetString("DeeplinkURLForNextSession", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string PLAYERPREFS_Experience
	{
		get
		{
			if (CachedPlayerPrefs.HasKey("Experience"))
			{
				return CachedPlayerPrefs.GetString("Experience");
			}
			return null;
		}
		set
		{
			CachedPlayerPrefs.SetString("Experience", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool PLAYERPREFS_HasSelectedInitialLanguage
	{
		get
		{
			return CachedPlayerPrefs.HasKey("HasDoneInitalLanguageSelect");
		}
		set
		{
			if (!value)
			{
				CachedPlayerPrefs.DeleteKey("HasDoneInitalLanguageSelect");
			}
			else
			{
				CachedPlayerPrefs.SetInt("HasDoneInitalLanguageSelect", 1);
			}
			CachedPlayerPrefs.Save();
		}
	}

	public static float TextboxScrollSensitivity
	{
		get
		{
			return CachedPlayerPrefs.GetFloat("TextboxScrollSensitivity", 1f);
		}
		set
		{
			CachedPlayerPrefs.SetFloat("TextboxScrollSensitivity", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectBattlefieldId
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectBattlefieldId", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectBattlefieldId", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectCardbackId
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectCardbackId", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectCardbackId", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectOpponentCardbackId
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectOpponentCardbackId", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectOpponentCardbackId", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectPetName
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectPetName", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectPetName", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectOpponentPetName
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectOpponentPetName", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectOpponentPetName", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectPetVariant
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectPetVariant", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectPetVariant", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectOpponentPetVariant
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectOpponentPetVariant", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectOpponentPetVariant", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static List<string> ReconnectEmotesList
	{
		get
		{
			return CachedPlayerPrefs.GetStringList("ReconnectEmotesList", null, '|');
		}
		set
		{
			CachedPlayerPrefs.SetStringList("ReconnectEmotesList", value, "|");
			CachedPlayerPrefs.Save();
		}
	}

	public static List<string> ReconnectOpponentEmotesList
	{
		get
		{
			return CachedPlayerPrefs.GetStringList("ReconnectOpponentEmotesList", null, '|');
		}
		set
		{
			CachedPlayerPrefs.SetStringList("ReconnectOpponentEmotesList", value, "|");
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectTitle
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectTitle", string.Empty);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectTitle", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectOpponentTitle
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectOpponentTitle", string.Empty);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectOpponentTitle", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectLimitedRankInfo
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectLimitedRankInfo", string.Empty);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectLimitedRankInfo", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string ReconnectConstructedRankInfo
	{
		get
		{
			return CachedPlayerPrefs.GetString("ReconnectConstructedRankInfo", string.Empty);
		}
		set
		{
			CachedPlayerPrefs.SetString("ReconnectConstructedRankInfo", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static string BundleSource
	{
		get
		{
			return CachedPlayerPrefs.GetString("BundleEndpoint", null);
		}
		set
		{
			CachedPlayerPrefs.SetString("BundleEndpoint", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static int SelectedBundleSourceHashCode
	{
		get
		{
			return CachedPlayerPrefs.GetInt("SelectedBundleSourceHashCode");
		}
		set
		{
			CachedPlayerPrefs.SetInt("SelectedBundleSourceHashCode", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool ShouldValidateServerCert
	{
		get
		{
			return PlayerPrefsExt.GetBool("CheckSC", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("CheckSC", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static int InactivityTimeoutMs
	{
		get
		{
			return CachedPlayerPrefs.GetInt("InactivityTimeoutMs", -1);
		}
		set
		{
			CachedPlayerPrefs.SetInt("InactivityTimeoutMs", -1);
			CachedPlayerPrefs.Save();
		}
	}

	public static string DoorbellOverrideContent
	{
		get
		{
			return PlayerPrefsExt.GetString("DoorbellOverrideContent");
		}
		set
		{
			PlayerPrefsExt.SetString("DoorbellOverrideContent", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool DoorbellOverrideToggle
	{
		get
		{
			if (!Debug.isDebugBuild)
			{
				return false;
			}
			return PlayerPrefsExt.GetBool("DoorbellOverrideToggle");
		}
		set
		{
			PlayerPrefsExt.SetBool("DoorbellOverrideToggle", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool IsBackgroundDownloadEnabled
	{
		get
		{
			return PlayerPrefsExt.GetBool("IsBackgroundDownloadEnabled");
		}
		set
		{
			PlayerPrefsExt.SetBool("IsBackgroundDownloadEnabled", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool KeepUnusedBundles
	{
		get
		{
			return PlayerPrefsExt.GetBool("KeepUnusedBundles");
		}
		set
		{
			PlayerPrefsExt.SetBool("KeepUnusedBundles", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static List<string> DebugLocalEventPaths
	{
		get
		{
			string text = CachedPlayerPrefs.GetString("DebugLocalEventPaths");
			if (string.IsNullOrEmpty(text))
			{
				return new List<string>();
			}
			return new List<string>(text.Split(','));
		}
		set
		{
			CachedPlayerPrefs.SetString("DebugLocalEventPaths", string.Join(",", value));
		}
	}

	public static List<string> DebugLocalCoursePaths
	{
		get
		{
			string text = CachedPlayerPrefs.GetString("DebugLocalCoursePaths");
			if (string.IsNullOrEmpty(text))
			{
				return new List<string>();
			}
			return new List<string>(text.Split(','));
		}
		set
		{
			CachedPlayerPrefs.SetString("DebugLocalCoursePaths", string.Join(",", value));
		}
	}

	public static List<string> DebugLocalCarouselPaths
	{
		get
		{
			string text = CachedPlayerPrefs.GetString("DebugLocalCarouselPaths");
			if (string.IsNullOrEmpty(text))
			{
				return new List<string>();
			}
			return new List<string>(text.Split(','));
		}
		set
		{
			CachedPlayerPrefs.SetString("DebugLocalCarouselPaths", string.Join(",", value));
		}
	}

	public static bool ForceUpdate
	{
		get
		{
			return PlayerPrefsExt.GetBool("Debug_ForceUpdate", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("Debug_ForceUpdate", value);
		}
	}

	public static bool RunningDSS
	{
		get
		{
			return PlayerPrefsExt.GetBool("RunningDSS", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("RunningDSS", value);
		}
	}

	public static string DSSReportName
	{
		get
		{
			return PlayerPrefsExt.GetString("DSSReportName");
		}
		set
		{
			PlayerPrefsExt.SetString("DSSReportName", value);
		}
	}

	public static string[] ExpandedDeckFolders
	{
		get
		{
			return GetUserString(UserPersonaID, "ExpandedDeckFolders")?.Split(',') ?? Array.Empty<string>();
		}
		set
		{
			SetUserString(UserPersonaID, "ExpandedDeckFolders", string.Join(",", value ?? Array.Empty<string>()));
		}
	}

	public static bool HasSeenPlayBlade
	{
		get
		{
			return PlayerPrefsExt.GetBool("HasSeenPlayBlade", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("HasSeenPlayBlade", value);
			CachedPlayerPrefs.Save();
		}
	}

	public static bool SaveDSReplays
	{
		get
		{
			return PlayerPrefsExt.GetBool("SaveDSReplay", defaultValue: false);
		}
		set
		{
			PlayerPrefsExt.SetBool("SaveDSReplay", value);
		}
	}

	public static string ReplayName
	{
		get
		{
			return PlayerPrefsExt.GetString("ReplayName");
		}
		set
		{
			PlayerPrefsExt.SetString("ReplayName", value);
		}
	}

	public static string QueuedAutoplayFile
	{
		get
		{
			return PlayerPrefsExt.GetString("QueuedAutoplayFile", null);
		}
		set
		{
			PlayerPrefsExt.SetString("QueuedAutoplayFile", value);
		}
	}

	public static int DebugAutoplayHighlightLogFontSize
	{
		get
		{
			return PlayerPrefsExt.GetInt("HighlightLogFontSize", 35);
		}
		set
		{
			PlayerPrefsExt.SetInt("HighlightLogFontSize", value);
		}
	}

	public static bool DisplayEnvironmentAndBundleEndpointSelectors
	{
		get
		{
			return PlayerPrefsExt.GetBool("DisplayEnvironmentAndBundleEndpointSelectors", defaultValue: true);
		}
		set
		{
			PlayerPrefsExt.SetBool("DisplayEnvironmentAndBundleEndpointSelectors", value);
		}
	}

	private static void OnAccountClientLoginStateChanged(LoginState loginState)
	{
		if (_accountClient != null)
		{
			_accountClient.LoginStateChanged -= OnAccountClientLoginStateChanged;
		}
		_accountClient = null;
	}

	public static void SetupVersion()
	{
		if (CachedPlayerPrefs.HasKey("PrefsVersion"))
		{
			_version = CachedPlayerPrefs.GetInt("PrefsVersion");
		}
		else
		{
			_version = 0;
		}
		CachedPlayerPrefs.SetInt("PrefsVersion", 1);
		if (_version < 1)
		{
			CachedPlayerPrefs.DeleteKey("HasDoneInitalLanguageSelect");
		}
	}

	public static string EncryptInMemoryData(string InBuffer)
	{
		try
		{
			if (InBuffer == null || InBuffer.Length <= 0)
			{
				return null;
			}
			int num = InBuffer.Length + InBuffer.Length % 16;
			if (num % 16 != 0)
			{
				num += 16 - num % 16;
			}
			_ = new byte[num];
			byte[] bytes = Encoding.ASCII.GetBytes(InBuffer.PadRight(num));
			ProtectedMemory.Protect(bytes, MemoryProtectionScope.SameLogon);
			return Convert.ToBase64String(bytes);
		}
		catch
		{
			return null;
		}
	}

	public static string DecryptInMemoryData(string InBuffer)
	{
		try
		{
			if (InBuffer == null || InBuffer.Length <= 0)
			{
				return null;
			}
			byte[] array = Convert.FromBase64String(InBuffer);
			ProtectedMemory.Unprotect(array, MemoryProtectionScope.SameLogon);
			return Encoding.ASCII.GetString(array).TrimEnd(' ');
		}
		catch
		{
			return null;
		}
	}

	public static string GetCampaignGraphSelectedEvent(string userAccountID, string graphName)
	{
		return GetUserString(userAccountID, graphName);
	}

	public static void SetCampaignGraphSelectedEvent(string userAccountID, string graphName, string eventName)
	{
		SetUserString(userAccountID, graphName, eventName);
	}

	public static string GetCampaignEventSelectedNode(string userAccountID, string graphName, string eventName)
	{
		return GetUserString(userAccountID, graphName + "." + eventName);
	}

	public static void SetCampaignEventSelectedNode(string userAccountID, string graphName, string eventName, string nodeName)
	{
		SetUserString(userAccountID, graphName + "." + eventName, nodeName);
	}

	public static void ClearCampaignEventSelectedNode(string userAccountID, string graphName, string eventName)
	{
		DeleteUserKey(userAccountID, graphName + "." + eventName);
	}

	public static string GetSelectedDeckId(string userAccountID, string eventName)
	{
		eventName = ((eventName == "Historic_Play") ? "Play" : eventName);
		return GetUserString(userAccountID, eventName + ".SelectedDeckId");
	}

	public static void SetSelectedDeckId(string userAccountID, string eventName, string value)
	{
		eventName = ((eventName == "Historic_Play") ? "Play" : eventName);
		SetUserString(userAccountID, eventName + ".SelectedDeckId", value);
	}

	public static void ClearSelectedDeckId(string userPlayfabId, string eventName)
	{
		DeleteUserKey(userPlayfabId, eventName + ".SelectedDeckId");
	}

	public static string GetStateMachineFlags(string playfabId)
	{
		return GetUserString(playfabId, "StateMachineFlags", string.Empty);
	}

	public static void SetStateMachineFlags(string playfabId, string value)
	{
		SetUserString(playfabId, "StateMachineFlags", value);
	}

	public static void ClearStateMachineFlags(string playfabId)
	{
		DeleteUserKey(playfabId, "StateMachineFlags");
	}

	public static string GetAcknowledgedBannedCardsForFormat(string playfabId, string formatName)
	{
		return GetUserString(playfabId, "BannedCardsAcknowledgedForFormat_" + formatName, string.Empty);
	}

	public static string GetAcknowledgedSuspendedCardsForFormat(string playfabId, string formatName)
	{
		return GetUserString(playfabId, "SuspendedCardsAcknowledgedForFormat_" + formatName, string.Empty);
	}

	public static string GetAcknowledgedRestrictedCardsForFormat(string playfabId, string formatName)
	{
		return GetUserString(playfabId, "RestrictedCardsAcknowledgedForFormat_" + formatName, string.Empty);
	}

	public static void SetAcknowledgedBannedCardsForFormat(string playfabId, string formatName, string collatedCardNames)
	{
		SetUserString(playfabId, "BannedCardsAcknowledgedForFormat_" + formatName, collatedCardNames);
	}

	public static void SetAcknowledgedSuspendedCardsForFormat(string playfabId, string formatName, string collatedCardNames)
	{
		SetUserString(playfabId, "SuspendedCardsAcknowledgedForFormat_" + formatName, collatedCardNames);
	}

	public static void SetAcknowledgedRestrictedCardsForFormat(string playfabId, string formatName, string collatedCardNames)
	{
		SetUserString(playfabId, "RestrictedCardsAcknowledgedForFormat_" + formatName, collatedCardNames);
	}

	public static bool GetRuleChangeViewed(string playfabId, string rule)
	{
		return GetUserBool(playfabId, "RuleChangeViewed_" + rule);
	}

	public static void SetRuleChangeViewed(string playfabId, string rule, bool value)
	{
		SetUserBool(playfabId, "RuleChangeViewed_" + rule, value);
	}

	public static bool GetHasSeenHandheldMOZTutorial(string userAccountID)
	{
		return GetUserBool(userAccountID, "HasSeenHandheldMOZTutorial");
	}

	public static void SetHasSeenHandheldMOZTutorial(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "HasSeenHandheldMOZTutorial", value);
	}

	public static string GetSetAnnouncementsViewed(string playfabId)
	{
		return GetUserString(playfabId, "SetAnnouncementsViewed", string.Empty);
	}

	public static void SetSetAnnouncementsViewed(string playfabId, string value)
	{
		SetUserString(playfabId, "SetAnnouncementsViewed", value);
	}

	public static void ClearSetAnnouncementsViewed(string playfabId)
	{
		DeleteUserKey(playfabId, "SetAnnouncementsViewed");
	}

	public static bool GetRotationEducationViewed(string playfabId, string renewalId)
	{
		return GetUserBool(playfabId, "RotationEducationViewed" + renewalId);
	}

	public static void SetRotationEducationViewed(string playfabId, string renewalId, bool value)
	{
		SetUserBool(playfabId, "RotationEducationViewed" + renewalId, value);
	}

	public static void ClearRotationEducationViewed(string playfabId, string renewalId)
	{
		DeleteUserKey(playfabId, "RotationEducationViewed" + renewalId);
	}

	public static bool GetOnboardingSkipped(string playfabId)
	{
		return GetUserBool(playfabId, "OnboardingSkipped");
	}

	public static void SetOnboardingSkipped(string playfabId, bool value)
	{
		SetUserBool(playfabId, "OnboardingSkipped", value);
	}

	public static string GetSelectedEventName(string userAccountID)
	{
		return GetUserString(userAccountID, "SelectedEvent");
	}

	public static void SetSelectedEventName(string userAccountID, string value)
	{
		value = ((value == "Historic_Play") ? "Play" : value);
		SetUserString(userAccountID, "SelectedEvent", value);
	}

	public static bool GetHasOpenedSealedPool(string userPlayfabId, Guid courseId)
	{
		return GetUserBool(userPlayfabId, courseId.ToString() + ".Event.HasOpenedSealedPool");
	}

	public static void SetHasOpenedSealedPool(string userPlayfabId, Guid courseId, bool value)
	{
		SetUserBool(userPlayfabId, courseId.ToString() + ".Event.HasOpenedSealedPool", value);
	}

	public static bool GetBoosterPackOpenAutoReveal()
	{
		return GetMachineBool("BoosterPackOpenAutoReveal");
	}

	public static void SetBoosterPackOpenAutoReveal(bool value)
	{
		SetMachineBool("BoosterPackOpenAutoReveal", value);
	}

	public static bool GetBoosterPackOpenSkipAnimation()
	{
		return GetMachineBool("BoosterPackOpenSkipAnimation");
	}

	public static void SetBoosterPackOpenSkipAnimation(bool value)
	{
		SetMachineBool("BoosterPackOpenSkipAnimation", value);
	}

	public static bool GetUseColumnView(string userAccountID)
	{
		return GetUserBool(userAccountID, "DeckBuilder.ColumnView");
	}

	public static void SetUseColumnView(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "DeckBuilder.ColumnView", value);
	}

	public static bool GetLargeCardsInPool(string userAccountID)
	{
		return GetUserBool(userAccountID, "DeckBuilder.LargeCardsInPool", defaultValue: true);
	}

	public static void SetLargeCardsInPool(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "DeckBuilder.LargeCardsInPool", value);
	}

	public static string GetDraftDeckSideboardIds(string courseId)
	{
		return CachedPlayerPrefs.GetString("Draft.DeckSideboardIds" + courseId, string.Empty);
	}

	public static void SetDraftDeckSideboardIds(string courseId, string value)
	{
		CachedPlayerPrefs.SetString("Draft.DeckSideboardIds" + courseId, value);
	}

	public static void DeleteDraftDeckSideboardIds(string courseId)
	{
		CachedPlayerPrefs.DeleteKey("Draft.DeckSideboardIds" + courseId);
	}

	public static bool GetDraftHasSeenVaultPopup(string userAccountID)
	{
		return GetUserBool(userAccountID, "Draft.HasSeenVaultPopup");
	}

	public static void SetDraftHasSeenVaultPopup(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "Draft.HasSeenVaultPopup", value);
	}

	public static bool GetHasSeenSideboardSubmitTip(string userAccountID)
	{
		return GetUserBool(userAccountID, "DeckBuilder.HasSeenSideboardSubmitTip");
	}

	public static void SetHasSeenSideboardSubmitTip(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "DeckBuilder.HasSeenSideboardSubmitTip", value);
	}

	public static bool GetHasSeenStore(string userAccountID)
	{
		return GetUserBool(userAccountID, "Store.HasUserEverUsedg");
	}

	public static void SetHasSeenStore(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "Store.HasUserEverUsedg", value);
	}

	public static bool GetStoreTermsAndConditions(string userAccountID)
	{
		return GetUserBool(userAccountID, "Store.TermsandConditions");
	}

	public static void SetStoreTermsAndConditions(string userAccountID, bool value)
	{
		SetUserBool(userAccountID, "Store.TermsandConditions", value);
	}

	public static string GetLastStoreViewed(string userAccountID)
	{
		return GetUserString(userAccountID, "Store.Viewed");
	}

	public static void SetLastStoreViewed(string userAccountID, string value)
	{
		SetUserString(userAccountID, "Store.Viewed", value);
	}

	public static string GetLastSetMasteryNavViewed(string userAccountID)
	{
		return GetUserString(userAccountID, "SetMastery.Viewed");
	}

	public static void SetLastSetMasteryNavViewed(string userAccountID, string value)
	{
		SetUserString(userAccountID, "SetMastery.Viewed", value);
	}

	public static string GetLastSetMasteryOrbSpendViewed(string userAccountID)
	{
		return GetUserString(userAccountID, "SetMastery.SpendHeat.Viewed");
	}

	public static void SetLastSetMasteryOrbSpendViewed(string userAccountID, string value)
	{
		SetUserString(userAccountID, "SetMastery.SpendHeat.Viewed", value);
	}

	public static string GetSelectedAvatar(string userAccountID)
	{
		return GetUserString(userAccountID, "Profile.SelectedAvatar");
	}

	public static string GetFavoriteSleeve(string userAccountID)
	{
		return GetUserString(userAccountID, "FavoriteSleeve", "CardBack_Default");
	}

	public static void SetFavoriteSleeve(string userAccountID, string value)
	{
		SetUserString(userAccountID, "FavoriteSleeve", value);
	}

	public static void ClearVanityItems(string userAccountID)
	{
		DeleteUserKey(userAccountID, "Profile.SelectedAccessory");
		DeleteUserKey(userAccountID, "Profile.SelectedAvatar");
		DeleteUserKey(userAccountID, "Profile.SelectedAccessoryMods");
		DeleteUserKey(userAccountID, "Profile.SelectedSleeve");
		CachedPlayerPrefs.Save();
	}

	public static bool GetUseVerboseLogs()
	{
		return GetMachineBool("UseVerboseLogs", Application.isEditor);
	}

	public static void SetUseVerboseLogs(bool newValue)
	{
		SetMachineBool("UseVerboseLogs", newValue);
	}

	public static bool GetHasEverForcedVerboseLogs()
	{
		return GetMachineBool("HasEverForcedVerbose");
	}

	public static void SetHasEverForcedVerboseLogs(bool newValue)
	{
		SetMachineBool("HasEverForcedVerbose", newValue);
	}

	public static bool GetShownQualifierBadge(string accountId)
	{
		return !string.IsNullOrEmpty(GetUserString(accountId, "ShownQualifierBadge"));
	}

	public static void SetShownQualifierBadge(string accountId, bool newValue)
	{
		if (newValue)
		{
			SetUserString(accountId, "ShownQualifierBadge", DateTime.Today.ToShortDateString());
		}
		else
		{
			DeleteUserKey(accountId, "ShownQualifierBadge");
		}
	}

	public static bool GetSparkRankRewardShown(string accountId)
	{
		return !string.IsNullOrEmpty(GetUserString(accountId, "SparkRankRewardShown"));
	}

	public static void SetSparkRankRewardShown(string accountId, bool newValue)
	{
		if (newValue)
		{
			SetUserString(accountId, "SparkRankRewardShown", DateTime.Today.ToShortDateString());
		}
		else
		{
			DeleteUserKey(accountId, "SparkRankRewardShown");
		}
	}

	public static bool GetSparkRankReturnHome(string accountId)
	{
		return GetUserBool(accountId, "SparkRankReturnHome");
	}

	public static void SetSparkRankReturnHome(string accountId, bool newValue)
	{
		SetUserBool(accountId, "SparkRankReturnHome", newValue);
	}

	public static string GetNPEObjectiveLastSeen(string accountId)
	{
		return GetUserString(accountId, "NPEObjectiveLastSeen");
	}

	public static void SetNPEObjectiveLastSeen(string accountId, string newValue)
	{
		SetUserString(accountId, "NPEObjectiveLastSeen", newValue);
	}

	public static void AddRecentChallenge(string yourUserAccountID, string theirUserAccountID)
	{
		if (!GetRecentChallenges(yourUserAccountID).Contains(theirUserAccountID))
		{
			int num = (GetUserInt(yourUserAccountID, "PrivateGameChallengeListIndex") + 1) % 5;
			SetUserString(yourUserAccountID, FormatChallengeString(num), theirUserAccountID);
			SetUserInt(yourUserAccountID, "PrivateGameChallengeListIndex", num);
			CachedPlayerPrefs.Save();
		}
	}

	private static string FormatChallengeString(int val)
	{
		return string.Format("{0}{1}", "PrivateGameChallenge", val);
	}

	private static void MigrateOldChallenges(string yourUserName)
	{
		for (int i = 0; i < 5; i++)
		{
			string key = FormatChallengeString(i);
			string text = CachedPlayerPrefs.GetString(key);
			if (!text.Equals(""))
			{
				int num = (GetUserInt(yourUserName, "PrivateGameChallengeListIndex") + 1) % 5;
				SetUserString(yourUserName, FormatChallengeString(num), text);
				SetUserInt(yourUserName, "PrivateGameChallengeListIndex", num);
				CachedPlayerPrefs.DeleteKey(key);
				CachedPlayerPrefs.Save();
			}
		}
		SetUserBool(yourUserName, "PrivateGameChallengesMigrated", value: true);
	}

	public static List<string> GetRecentChallenges(string yourUserName)
	{
		if (!GetUserBool(yourUserName, "PrivateGameChallengesMigrated"))
		{
			MigrateOldChallenges(yourUserName);
		}
		List<string> list = new List<string>();
		for (int i = 0; i < 5; i++)
		{
			string userString = GetUserString(yourUserName, FormatChallengeString(i));
			if (!string.IsNullOrEmpty(userString))
			{
				list.Add(userString);
			}
		}
		return list;
	}

	public static void UpdateNPEGameAttemptNumber(string userAccountID, int gameNumber)
	{
		string settingName = "NPEGame" + gameNumber + "Attempt";
		int value = GetUserInt(userAccountID, settingName) + 1;
		SetUserInt(userAccountID, settingName, value);
	}

	public static int GetNPEGameAttemptNumber(string userAccountID, int gameNumber)
	{
		string settingName = "NPEGame" + gameNumber + "Attempt";
		return GetUserInt(userAccountID, settingName);
	}

	public static bool CheckIfInExperimentalGroup_Experiment002(string userAccountID)
	{
		return GetUserBool(userAccountID, "Ex002PLUL3");
	}

	public static void AddUserToExperimentalGroup_Experiment002(string userAccountID)
	{
		SetUserBool(userAccountID, "Ex002PLUL3", value: true);
		CachedPlayerPrefs.Save();
	}

	public static bool CheckIfInExperimentalGroup_Experiment003(string userAccountID)
	{
		return GetUserBool(userAccountID, "Ex003NSBG1AG2");
	}

	public static void AddUserToExperimentalGroup_Experiment003(string userAccountID)
	{
		SetUserBool(userAccountID, "Ex003NSBG1AG2", value: true);
		CachedPlayerPrefs.Save();
	}

	private static void DeleteUserKey(string userAccountID, string settingName)
	{
		if (!string.IsNullOrEmpty(userAccountID))
		{
			CachedPlayerPrefs.DeleteKey(userAccountID + "." + settingName);
		}
	}

	private static string GetUserString(string userAccountID, string settingName, string defaultValue = null)
	{
		if (string.IsNullOrEmpty(userAccountID))
		{
			return defaultValue;
		}
		return CachedPlayerPrefs.GetString(userAccountID + "." + settingName, defaultValue ?? string.Empty);
	}

	private static void SetUserString(string userAccountID, string settingName, string value)
	{
		if (!string.IsNullOrEmpty(userAccountID))
		{
			CachedPlayerPrefs.SetString(userAccountID + "." + settingName, value);
			CachedPlayerPrefs.Save();
		}
	}

	private static float GetUserFloat(string userAccountID, string settingName, float defaultValue = 0f)
	{
		if (string.IsNullOrEmpty(userAccountID))
		{
			return defaultValue;
		}
		return CachedPlayerPrefs.GetFloat(userAccountID + "." + settingName, defaultValue);
	}

	private static void SetUserFloat(string userAccountID, string settingName, float value)
	{
		if (!string.IsNullOrEmpty(userAccountID))
		{
			CachedPlayerPrefs.SetFloat($"{userAccountID}.{settingName}", value);
			CachedPlayerPrefs.Save();
		}
	}

	private static int GetUserInt(string userAccountID, string settingName, int defaultValue = 0)
	{
		if (string.IsNullOrEmpty(userAccountID))
		{
			return defaultValue;
		}
		return CachedPlayerPrefs.GetInt($"{userAccountID}.{settingName}", defaultValue);
	}

	private static void SetUserInt(string userAccountID, string settingName, int value)
	{
		if (!string.IsNullOrEmpty(userAccountID))
		{
			CachedPlayerPrefs.SetInt($"{userAccountID}.{settingName}", value);
			CachedPlayerPrefs.Save();
		}
	}

	private static bool GetUserBool(string userAccountID, string settingName, bool defaultValue = false)
	{
		return GetUserInt(userAccountID, settingName, defaultValue ? 1 : 0) == 1;
	}

	private static void SetUserBool(string userAccountID, string settingName, bool value)
	{
		SetUserInt(userAccountID, settingName, value ? 1 : 0);
	}

	private static bool GetMachineBool(string settingName, bool defaultValue = false)
	{
		int defaultValue2 = (defaultValue ? 1 : 0);
		return CachedPlayerPrefs.GetInt(settingName, defaultValue2) == 1;
	}

	private static void SetMachineBool(string settingName, bool value)
	{
		int value2 = (value ? 1 : 0);
		CachedPlayerPrefs.SetInt(settingName, value2);
		CachedPlayerPrefs.Save();
	}

	public static string GetRefreshToken(string optionalsuffix = "")
	{
		string text = CachedPlayerPrefs.GetString("WAS-RefreshTokenE" + optionalsuffix, null);
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = DecryptInMemoryData(text);
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
		}
		return CachedPlayerPrefs.GetString("WAS-RefreshToken" + optionalsuffix, null);
	}

	public static void SetRefreshToken(string newtoken, string optionalsuffix = "")
	{
		CachedPlayerPrefs.SetString("WAS-RefreshTokenE" + optionalsuffix, EncryptInMemoryData(newtoken));
		CachedPlayerPrefs.Save();
	}
}
