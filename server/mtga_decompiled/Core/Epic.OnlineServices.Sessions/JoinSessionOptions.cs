namespace Epic.OnlineServices.Sessions;

public class JoinSessionOptions
{
	public int ApiVersion => 1;

	public string SessionName { get; set; }

	public SessionDetails SessionHandle { get; set; }

	public ProductUserId LocalUserId { get; set; }
}
