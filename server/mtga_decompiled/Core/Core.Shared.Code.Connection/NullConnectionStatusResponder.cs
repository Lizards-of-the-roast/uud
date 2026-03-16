namespace Core.Shared.Code.Connection;

public class NullConnectionStatusResponder : IConnectionStatusResponder
{
	public void OnDoorbellError()
	{
	}

	public void OnConnectionClosedByIdleTimeout()
	{
	}

	public void OnConnectionClosedByServer()
	{
	}

	public void OnReconnectFailed()
	{
	}

	public void NoActiveMatchFound()
	{
	}

	public void OnMatchReconnectFailed()
	{
	}
}
