using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullPlayerNumericAidController : IPlayerNumericAidController
{
	public static readonly IPlayerNumericAidController Default = new NullPlayerNumericAidController();

	public void Add(uint playerId, PlayerNumericAid playerNumericAid)
	{
	}

	public void Update(uint playerId, PlayerNumericAid playerNumericAid)
	{
	}

	public void Remove(uint playerId, PlayerNumericAid playerNumericAid)
	{
	}
}
