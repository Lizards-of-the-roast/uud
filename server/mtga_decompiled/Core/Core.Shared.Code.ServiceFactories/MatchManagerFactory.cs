namespace Core.Shared.Code.ServiceFactories;

public class MatchManagerFactory
{
	public static MatchManager Create()
	{
		return new MatchManager(new UnityCrossThreadLogger());
	}
}
