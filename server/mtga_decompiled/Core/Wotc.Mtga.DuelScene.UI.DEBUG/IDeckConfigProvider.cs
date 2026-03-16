using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface IDeckConfigProvider
{
	IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> GetAllDecks();
}
