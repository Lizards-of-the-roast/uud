using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface ICardStackWorkflow
{
	bool CanStack(ICardDataAdapter lhs, ICardDataAdapter rhs);
}
