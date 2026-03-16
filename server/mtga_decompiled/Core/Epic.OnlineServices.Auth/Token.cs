namespace Epic.OnlineServices.Auth;

public class Token
{
	public int ApiVersion => 1;

	public string App { get; set; }

	public string ClientId { get; set; }

	public EpicAccountId AccountId { get; set; }

	public string AccessToken { get; set; }

	public double ExpiresIn { get; set; }

	public string ExpiresAt { get; set; }

	public AuthTokenType AuthType { get; set; }
}
