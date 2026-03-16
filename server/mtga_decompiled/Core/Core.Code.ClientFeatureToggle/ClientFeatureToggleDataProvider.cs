using System;
using System.Collections.Generic;
using Wizards.Mtga;
using Wizards.Mtga.Models.ClientModels;
using Wizards.Unification.Models.FrontDoor;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Code.ClientFeatureToggle;

public class ClientFeatureToggleDataProvider : IDisposable
{
	private readonly Dictionary<string, IClientFeatureToggle> _toggles = new Dictionary<string, IClientFeatureToggle>
	{
		{
			"ClientPlayerInbox",
			new RoleOverrideFeatureToggle(defaultToggleValue: true, UserHasDebugging, UserHasWotcAccess)
		},
		{
			"NPE_V2",
			new ClientFeatureToggleBase(defaultToggleValue: true)
		},
		{
			"SteamSocialAccounts",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"ShowAllActiveEvents",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"BiMessageTransition",
			new ClientFeatureToggleBase(defaultToggleValue: true)
		},
		{
			"ClientPrizeWall",
			new ClientFeatureToggleBase(defaultToggleValue: true)
		},
		{
			"AchievementSceneStatus",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"PrizeWallTixShouldNotify",
			new ClientFeatureToggleBase(defaultToggleValue: true)
		},
		{
			"Lobby",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"RingDoorbellV2",
			new ClientFeatureToggleBase(defaultToggleValue: true)
		},
		{
			"Tournament",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"MP_A",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"MP_B",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"SocialV2Status",
			new ClientFeatureToggleBase(defaultToggleValue: false)
		},
		{
			"Challenges",
			new ClientFeatureToggleBase(defaultToggleValue: true)
		}
	};

	private IFrontDoorConnectionServiceWrapper _frontDoorConnectionService;

	private bool _initialized;

	private Action _featureTogglesUpdated;

	public IEnumerable<string> AllToggleKeys => _toggles.Keys;

	public static ClientFeatureToggleDataProvider Create()
	{
		return new ClientFeatureToggleDataProvider();
	}

	private ClientFeatureToggleDataProvider()
	{
		ConsumeOverrideConfig();
	}

	public bool IsInitialized()
	{
		return _initialized;
	}

	public void InjectFrontDoor(IFrontDoorConnectionServiceWrapper frontDoorConnection)
	{
		_frontDoorConnectionService = frontDoorConnection;
		_frontDoorConnectionService.FDCAWS.OnMsg_KillSwitchNotification += OnKillSwitchNotification;
		ConsumeKillSwitchConfig();
		_initialized = true;
		FeatureTogglesUpdated();
	}

	public bool GetToggleValueById(string id)
	{
		if (_toggles.TryGetValue(id, out var value))
		{
			return value.GetToggleValue();
		}
		return false;
	}

	public void RegisterForToggleUpdates(Action callback)
	{
		_featureTogglesUpdated = (Action)Delegate.Combine(_featureTogglesUpdated, callback);
	}

	public void UnRegisterForToggleUpdates(Action callback)
	{
		_featureTogglesUpdated = (Action)Delegate.Remove(_featureTogglesUpdated, callback);
	}

	private void FeatureTogglesUpdated()
	{
		if (_featureTogglesUpdated != null)
		{
			_featureTogglesUpdated();
		}
	}

	private void ConsumeOverrideConfig()
	{
		foreach (KeyValuePair<string, IClientFeatureToggle> toggle in _toggles)
		{
			if (OverridesConfiguration.Local.HasFeatureToggleValue(toggle.Key))
			{
				toggle.Value.SetOverrideConfigValue(OverridesConfiguration.Local.GetFeatureToggleValue(toggle.Key));
			}
		}
	}

	private void ConsumeKillSwitchConfig()
	{
		if (_frontDoorConnectionService == null || _frontDoorConnectionService.Killswitch == null)
		{
			return;
		}
		foreach (KeyValuePair<string, IClientFeatureToggle> toggle in _toggles)
		{
			string key = MapKeyToKillswitchName(toggle.Key);
			bool value2;
			if (_frontDoorConnectionService.Killswitch.KillSwitches.TryGetValue(key, out var value))
			{
				toggle.Value.SetKillSwitchValue(!value);
			}
			else if (_frontDoorConnectionService.Killswitch.UxKillSwitches.TryGetValue(key, out value2))
			{
				toggle.Value.SetKillSwitchValue(!value2);
			}
			else
			{
				toggle.Value.ClearKillSwitchValue();
			}
		}
	}

	private void OnKillSwitchNotification(Client_KillSwitchNotification killSwitchNotification)
	{
		ConsumeKillSwitchConfig();
		FeatureTogglesUpdated();
	}

	private string MapKeyToKillswitchName(string toggleId)
	{
		return toggleId switch
		{
			"ClientPlayerInbox" => ESystemStatusNames.InboxStatus.ToString(), 
			"AchievementSceneStatus" => ESystemStatusNames.AchievementSceneStatus.ToString(), 
			"Lobby" => ESystemStatusNames.LobbyServiceStatus.ToString(), 
			"Tournament" => ESystemStatusNames.TournamentStatus.ToString(), 
			"SocialV2Status" => ESystemStatusNames.SocialV2Status.ToString(), 
			"Challenges" => ESystemStatusNames.ChallengeStatus.ToString(), 
			_ => toggleId, 
		};
	}

	public static bool UserHasDebugging()
	{
		return Pantry.Get<IAccountClient>().AccountInformation.HasRole_Debugging();
	}

	public static bool UserHasWotcAccess()
	{
		return Pantry.Get<IAccountClient>().AccountInformation.HasRole_WotCAccess();
	}

	public void Dispose()
	{
		_initialized = false;
		_featureTogglesUpdated = null;
	}
}
