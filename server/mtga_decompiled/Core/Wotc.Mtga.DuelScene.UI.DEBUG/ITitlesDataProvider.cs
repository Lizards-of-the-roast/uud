using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface ITitlesDataProvider
{
	IReadOnlyList<string> GetAllTitles();
}
