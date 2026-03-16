using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface ISecondaryLayoutIdListProvider
{
	IEnumerable<uint> GetSecondaryLayoutIds();
}
