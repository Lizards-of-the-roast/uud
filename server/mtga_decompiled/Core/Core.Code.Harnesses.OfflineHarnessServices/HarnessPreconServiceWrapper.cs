using System;
using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessPreconServiceWrapper : IPreconDeckServiceWrapper
{
	public Client_Deck GetPreconDeck(Guid id)
	{
		throw new NotImplementedException();
	}

	public Client_Deck GetPreconDeckByDescription(string description)
	{
		throw new NotImplementedException();
	}

	public Promise<Dictionary<Guid, Client_Deck>> EnsurePreconDecks()
	{
		throw new NotImplementedException();
	}
}
