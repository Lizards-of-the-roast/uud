using Core.Code.ClientFeatureToggle;
using Steamworks;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;

namespace _3rdParty.Steam;

public class SteamLoginContext : ILoginContext
{
	public bool IsLoggedIn
	{
		get
		{
			if (Steam.LoggedIn && !string.IsNullOrEmpty(SocialToken))
			{
				return Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("SteamSocialAccounts");
			}
			return false;
		}
	}

	public string SocialToken => Steam.AuthToken;

	public string SocialType => "steam";

	public string SocialId => SteamClient.SteamId.ToString();

	public string ClientID { get; set; }

	public string ClientSecret { get; set; }
}
