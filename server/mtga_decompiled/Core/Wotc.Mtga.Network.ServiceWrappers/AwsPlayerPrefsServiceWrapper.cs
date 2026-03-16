using System.Collections.Generic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.FrontDoor;
using Wizards.Unification.Models.PlayBlade;

namespace Wotc.Mtga.Network.ServiceWrappers;

public class AwsPlayerPrefsServiceWrapper : IPlayerPrefsServiceWrapper
{
	private readonly FrontDoorConnectionAWS _fdc;

	public AwsPlayerPrefsServiceWrapper(FrontDoorConnectionAWS fdcAWS)
	{
		_fdc = fdcAWS;
	}

	public Promise<DTO_PlayerPreferences> GetPlayerPreferences()
	{
		return _fdc.SendMessage<DTO_PlayerPreferences>(CmdType.GetPlayerPreferences, (object)null);
	}

	public Promise<DTO_PlayerPreferences> SetPlayerPreferences(Dictionary<string, string> prefs)
	{
		SetPlayerPreferencesReq request = new SetPlayerPreferencesReq
		{
			Preferences = new DTO_PlayerPreferences
			{
				Preferences = prefs
			}
		};
		return _fdc.SendMessage<DTO_PlayerPreferences>(CmdType.SetPlayerPreferences, request);
	}
}
