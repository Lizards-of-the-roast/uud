namespace Core.Shared.Code.Connection;

public interface IConnectionStatusResponder
{
	void OnDoorbellError();

	void OnConnectionClosedByIdleTimeout();

	void OnConnectionClosedByServer();

	void OnReconnectFailed();

	void NoActiveMatchFound();

	void OnMatchReconnectFailed();
}
