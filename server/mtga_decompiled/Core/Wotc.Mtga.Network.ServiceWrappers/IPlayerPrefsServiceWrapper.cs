using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.PlayBlade;

namespace Wotc.Mtga.Network.ServiceWrappers;

public interface IPlayerPrefsServiceWrapper
{
	Promise<DTO_PlayerPreferences> GetPlayerPreferences();

	Promise<DTO_PlayerPreferences> SetPlayerPreferences(Dictionary<string, string> prefs);
}
