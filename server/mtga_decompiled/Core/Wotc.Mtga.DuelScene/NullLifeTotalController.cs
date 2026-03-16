namespace Wotc.Mtga.DuelScene;

public class NullLifeTotalController : ILifeTotalController
{
	public static readonly ILifeTotalController Default = new NullLifeTotalController();

	public void IncrementPlayerLife(uint playerId, int amount)
	{
	}
}
