using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullPlayerAbilityController : IPlayerAbilityController
{
	public static readonly IPlayerAbilityController Default = new NullPlayerAbilityController();

	public void AddAbility(uint playerId, AddedAbilityData addedAbilityData)
	{
	}

	public void RemoveAbility(uint playerId, AddedAbilityData addedAbilityData)
	{
	}
}
