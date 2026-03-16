using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.PrivateGame;

public class ChallengeDataProvider : IDisposable
{
	public enum ChallengePermissionState
	{
		None,
		Normal,
		Restricted_InMatch,
		Restricted_MaxOutgoingChallengesReached,
		Restricted_ChallengesKillswitched
	}

	private PlayerPrefsDataProvider _playerPrefsDataProvider;

	private Action<PVPChallengeData> _onPVPChallengeDataChange;

	private Action<PVPChallengeData> _onPVPChallengeDataChangeMainThread;

	private Dictionary<Guid, PVPChallengeData> _challenges = new Dictionary<Guid, PVPChallengeData>();

	public ChallengePermissionState PermissionState;

	public bool Initialized { get; private set; }

	public bool PreferencesAvailable
	{
		get
		{
			if (_playerPrefsDataProvider != null)
			{
				return _playerPrefsDataProvider.Initialized;
			}
			return false;
		}
	}

	public static ChallengeDataProvider Create()
	{
		return new ChallengeDataProvider(Pantry.Get<PlayerPrefsDataProvider>());
	}

	public ChallengeDataProvider(PlayerPrefsDataProvider playerPrefsDataProvider)
	{
		_playerPrefsDataProvider = playerPrefsDataProvider;
		Initialized = true;
	}

	public Promise<bool> GetBlockNonFriendChallenges()
	{
		if (PreferencesAvailable)
		{
			return _playerPrefsDataProvider.GetPreferenceBool("BlockNonFriendChallenges");
		}
		return new SimplePromise<bool>(result: false);
	}

	public Promise<bool> SetBlockNonFriendChallenges(bool value)
	{
		if (PreferencesAvailable)
		{
			return _playerPrefsDataProvider.SetPreferenceBool("BlockNonFriendChallenges", value).Convert(delegate
			{
				bool bResult = false;
				return GetBlockNonFriendChallenges().Convert(delegate(bool y)
				{
					if (y == value)
					{
						bResult = true;
					}
					return bResult;
				}).Result;
			});
		}
		return new SimplePromise<bool>(result: false);
	}

	public Dictionary<Guid, PVPChallengeData> GetAllChallenges()
	{
		return _challenges;
	}

	public void SetChallengeData(PVPChallengeData challengeData)
	{
		if (challengeData != null)
		{
			_challenges[challengeData.ChallengeId] = challengeData;
			BroadcastChallengeDataChange(challengeData);
		}
	}

	public void RemoveChallengeData(Guid challengeId)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData != null && _challenges.Remove(challengeId))
		{
			challengeData.Status = ChallengeStatus.Removed;
			BroadcastChallengeDataChange(challengeData);
		}
	}

	public bool HasChallengeForPlayer(string playerId, out PVPChallengeData challengeData)
	{
		challengeData = null;
		if (!string.IsNullOrEmpty(playerId))
		{
			foreach (KeyValuePair<Guid, PVPChallengeData> challenge in _challenges)
			{
				if (challenge.Value.ChallengePlayers.ContainsKey(playerId) || challenge.Value.Invites.FirstOrDefault((KeyValuePair<string, ChallengeInvite> invitePair) => invitePair.Value.Recipient.PlayerId == playerId).Value != null)
				{
					challengeData = challenge.Value;
					return true;
				}
			}
		}
		return false;
	}

	public PVPChallengeData GetChallengeData(string playerId)
	{
		if (HasChallengeForPlayer(playerId, out var challengeData))
		{
			return challengeData;
		}
		return null;
	}

	public PVPChallengeData GetChallengeData(Guid challengeId)
	{
		return _challenges.GetValueOrDefault(challengeId);
	}

	public void RegisterForChallengeChanges(Action<PVPChallengeData> handler, bool forceOnMainThread = true)
	{
		if (forceOnMainThread)
		{
			_onPVPChallengeDataChangeMainThread = (Action<PVPChallengeData>)Delegate.Combine(_onPVPChallengeDataChangeMainThread, handler);
		}
		else
		{
			_onPVPChallengeDataChange = (Action<PVPChallengeData>)Delegate.Combine(_onPVPChallengeDataChange, handler);
		}
	}

	public void UnRegisterForChallengeChanges(Action<PVPChallengeData> handler)
	{
		_onPVPChallengeDataChange = (Action<PVPChallengeData>)Delegate.Remove(_onPVPChallengeDataChange, handler);
		_onPVPChallengeDataChangeMainThread = (Action<PVPChallengeData>)Delegate.Remove(_onPVPChallengeDataChangeMainThread, handler);
	}

	public void BroadcastChallengeDataChange(PVPChallengeData data)
	{
		try
		{
			_onPVPChallengeDataChange?.Invoke(data);
			if (_onPVPChallengeDataChangeMainThread != null)
			{
				MainThreadDispatcher.Instance.Add(delegate
				{
					_onPVPChallengeDataChangeMainThread?.Invoke(data);
				});
			}
		}
		catch (Exception e)
		{
			SimpleLog.LogException(e);
		}
	}

	public void Dispose()
	{
		_challenges.Clear();
		_onPVPChallengeDataChange = null;
		_onPVPChallengeDataChangeMainThread = null;
	}
}
