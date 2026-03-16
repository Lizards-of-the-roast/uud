using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IClientSideOptionModelMutator
{
	bool Mutate(ref ICardDataAdapter targetModel);
}
