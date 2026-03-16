using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IRelatedCardIdProvider
{
	IEnumerable<uint> GetRelatedIds(DuelScene_CDC card);
}
