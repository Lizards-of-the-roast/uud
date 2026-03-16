using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface ISleeveDataProvider
{
	IReadOnlyList<string> GetAllSleeves();
}
