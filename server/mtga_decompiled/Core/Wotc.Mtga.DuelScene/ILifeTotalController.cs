namespace Wotc.Mtga.DuelScene;

public interface ILifeTotalController
{
	void IncrementPlayerLife(uint playerId, int amount);
}
