using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IPlayerNumericAidController
{
	void Add(uint playerId, PlayerNumericAid playerNumericAid);

	void Update(uint playerId, PlayerNumericAid playerNumericAid);

	void Remove(uint playerId, PlayerNumericAid playerNumericAid);
}
