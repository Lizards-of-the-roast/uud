using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IFakeCardViewProvider
{
	IEnumerable<DuelScene_CDC> GetAllFakeCards();

	DuelScene_CDC GetFakeCard(string key);

	bool TryGetFakeCard(string key, out DuelScene_CDC fakeCdc)
	{
		fakeCdc = GetFakeCard(key);
		return fakeCdc != null;
	}
}
