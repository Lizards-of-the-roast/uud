using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface ISortedBrowser
{
	void Sort(List<DuelScene_CDC> toSort);
}
