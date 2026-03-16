using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullRevealedCardsManager : IRevealedCardsManager, IRevealedCardsProvider, IRevealedCardsController
{
	public static readonly IRevealedCardsManager Default = new NullRevealedCardsManager();

	private readonly IRevealedCardsProvider _provider = NullRevealedCardsProvider.Default;

	private readonly IRevealedCardsController _controller = NullRevealedCardsController.Default;

	public MtgCardInstance GetCardRevealed(uint instanceId)
	{
		return _provider.GetCardRevealed(instanceId);
	}

	public ICardHolder GetAssociatedCardHolder(uint instanceId)
	{
		return _provider.GetAssociatedCardHolder(instanceId);
	}

	public void CreateRevealedCard(uint ownerId, MtgCardInstance instance, DuelScene_CDC applyTo)
	{
		_controller.CreateRevealedCard(ownerId, instance, applyTo);
	}

	public void UpdateRevealedCard(MtgCardInstance instance)
	{
		_controller.UpdateRevealedCard(instance);
	}

	public void DeleteRevealedCard(uint instanceId)
	{
		_controller.DeleteRevealedCard(instanceId);
	}
}
