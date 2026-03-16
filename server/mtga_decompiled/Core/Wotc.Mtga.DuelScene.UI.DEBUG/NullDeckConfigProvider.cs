using System.Collections.Generic;
using GreClient.Network;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullDeckConfigProvider : IDeckConfigProvider
{
	public static readonly IDeckConfigProvider Default = new NullDeckConfigProvider();

	public IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> GetAllDecks()
	{
		return DictionaryExtensions.Empty<string, IReadOnlyList<DeckConfig>>();
	}
}
