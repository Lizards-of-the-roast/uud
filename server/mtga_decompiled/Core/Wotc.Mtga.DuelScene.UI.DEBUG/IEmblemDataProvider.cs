using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface IEmblemDataProvider
{
	IReadOnlyList<EmblemData> GetAllEmblems();
}
