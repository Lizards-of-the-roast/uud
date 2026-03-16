using GreClient.Network;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public static class IDeckConfigProviderExtensions
{
	public static DeckConfig GetDefaultDeck(this IDeckConfigProvider deckConfigProvider)
	{
		if (deckConfigProvider.GetAllDecks().TryGetValue(string.Empty, out var value))
		{
			foreach (DeckConfig item in value)
			{
				if (item.Name == "FallbackDeck")
				{
					return item;
				}
			}
		}
		return DeckConfig.Default();
	}
}
