using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IRevealedCardsController
{
	void CreateRevealedCard(uint ownerId, MtgCardInstance instance, DuelScene_CDC applyTo = null);

	void UpdateRevealedCard(MtgCardInstance instance);

	void DeleteRevealedCard(uint instanceId);
}
