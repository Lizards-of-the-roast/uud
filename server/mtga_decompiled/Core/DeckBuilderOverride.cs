using System.Collections.Generic;

public readonly struct DeckBuilderOverride
{
	public readonly Dictionary<uint, uint> CardPool;

	public readonly Dictionary<uint, string> CardSkins;

	public DeckBuilderOverride(Dictionary<uint, uint> pool, Dictionary<uint, string> skins)
	{
		CardPool = pool;
		CardSkins = skins;
	}
}
