using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public abstract class BaseUserRequestDebugRenderer
{
	public abstract void Render();
}
public abstract class BaseUserRequestDebugRenderer<TRequest> : BaseUserRequestDebugRenderer where TRequest : BaseUserRequest
{
	protected readonly TRequest _request;

	protected BaseUserRequestDebugRenderer(TRequest request)
	{
		_request = request;
	}
}
