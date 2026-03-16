namespace Wizards.Mtga.Platforms.EpicOnlineService;

public class EpicLoginContext : ILoginContext
{
	public bool IsLoggedIn => EOSClient.State == EOSClientState.LoggedIn;

	public string SocialToken => EOSClient.AuthToken.AccessToken;

	public string SocialType => "epic";

	public string SocialId => EOSClient.AccountId.ToString();

	public string ClientID { get; set; }

	public string ClientSecret { get; set; }
}
