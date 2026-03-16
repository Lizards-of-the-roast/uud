namespace Core.Shared.Code.Connection;

public interface IConnectionIndicator
{
	void ShowReconnectIndicator(bool shouldEnable);
}
