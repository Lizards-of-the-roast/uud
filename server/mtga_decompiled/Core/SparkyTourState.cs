using System.Threading.Tasks;
using Core.Meta.NewPlayerExperience.Graph;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;

public class SparkyTourState
{
	private const RankingClassType MIN_RANK_FOR_SET_ANNOUNCE = RankingClassType.Bronze;

	private const RankingClassType MIN_RANK_FOR_BANNED_CARDS = RankingClassType.Bronze;

	private readonly NewPlayerExperienceStrategy _newPlayerExperienceStrategy = Pantry.Get<NewPlayerExperienceStrategy>();

	private readonly IColorChallengeStrategy _colorChallengeStrategy = Pantry.Get<IColorChallengeStrategy>();

	private readonly IPlayerRankServiceWrapper _rankService = Pantry.Get<IPlayerRankServiceWrapper>();

	private NPEState _npeState;

	public bool StateLoaded => _colorChallengeStrategy.Initialized;

	public bool ClientForcedToUnlock => _colorChallengeStrategy.ColorChallengeSkipped;

	public int FinishedColorMasteryEvents => _colorChallengeStrategy.CompletedTracks.Count;

	public Task<bool> GetColorMasteryEventComplete => _colorChallengeStrategy.GetMilestoneStatus("ColorChallengeComplete");

	public bool SetAnnounceTrailerUnlocked
	{
		get
		{
			if (!ClientForcedToUnlock)
			{
				return _rankService.CombinedRank.constructed.rankClass >= RankingClassType.Bronze;
			}
			return true;
		}
	}

	public bool BannedCardsPopupUnlocked
	{
		get
		{
			if (!ClientForcedToUnlock)
			{
				return _rankService.CombinedRank.constructed.rankClass >= RankingClassType.Bronze;
			}
			return true;
		}
	}

	public void Init(NPEState npeState)
	{
		_npeState = npeState;
	}

	public async Task<bool> PVPGamesLocked()
	{
		if (ClientForcedToUnlock)
		{
			return false;
		}
		return !(await _newPlayerExperienceStrategy.OpenedDualColorPreconEvent);
	}

	public async Task<bool> PVPRankedGamesUnlocked()
	{
		if (ClientForcedToUnlock)
		{
			return true;
		}
		return await _newPlayerExperienceStrategy.SparkQueueOpened;
	}

	public async Task<bool> EventsUnlocked()
	{
		if (ClientForcedToUnlock)
		{
			return true;
		}
		return await _newPlayerExperienceStrategy.NpeCompleted;
	}

	public async Task<bool> GetEligibleForStoreUpdates()
	{
		if (ClientForcedToUnlock)
		{
			return true;
		}
		return await _newPlayerExperienceStrategy.GraduatedSparkQueue;
	}

	public async Task SkipTour()
	{
		_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.Onboarding_Skipped));
		await UnlockClient();
		string userId = GetUserId();
		string stateMachineFlags = MDNPlayerPrefs.GetStateMachineFlags(userId);
		MDNPlayerPrefs.SetStateMachineFlags(userId, string.Join(",", stateMachineFlags) + ",NPEOnboarding_Base:EPP_GraduationDone");
		AudioManager.PlayAudio(WwiseEvents.sparky_global_stop, AudioManager.Default);
	}

	private static string GetUserId()
	{
		string text = Pantry.Get<IAccountClient>()?.AccountInformation?.PersonaID;
		if (text == null)
		{
			text = "default";
			Debug.LogError("Failed to get UserId for NPE state machine flags. Some or all state machine flags will be stored/retrieved via the default user, specific only to this device.");
		}
		return text;
	}

	private Task UnlockClient()
	{
		return _newPlayerExperienceStrategy.Skip().AsPromise().IfSuccess(delegate
		{
			_colorChallengeStrategy.Skip();
		})
			.AsTask;
	}
}
