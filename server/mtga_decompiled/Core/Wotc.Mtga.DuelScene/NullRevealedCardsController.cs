using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullRevealedCardsController : IRevealedCardsController
{
	public static readonly IRevealedCardsController Default = new NullRevealedCardsController();

	public void CreateRevealedCard(uint ownerId, MtgCardInstance instance, DuelScene_CDC applyTo = null)
	{
	}

	public void UpdateRevealedCard(MtgCardInstance instance)
	{
	}

	public void DeleteRevealedCard(uint instanceId)
	{
	}
}
