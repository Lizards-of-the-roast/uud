using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class MutableFakeCardViewProvider : IFakeCardViewProvider
{
	public readonly Dictionary<string, DuelScene_CDC> FakeCards = new Dictionary<string, DuelScene_CDC>();

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		foreach (KeyValuePair<string, DuelScene_CDC> fakeCard in FakeCards)
		{
			yield return fakeCard.Value;
		}
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		if (!FakeCards.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}
}
