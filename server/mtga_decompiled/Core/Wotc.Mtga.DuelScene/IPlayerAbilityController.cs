using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IPlayerAbilityController
{
	void AddAbility(uint playerId, AddedAbilityData addedAbilityData);

	void RemoveAbility(uint playerId, AddedAbilityData addedAbilityData);
}
