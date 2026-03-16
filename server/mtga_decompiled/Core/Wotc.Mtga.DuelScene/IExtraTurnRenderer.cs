using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IExtraTurnRenderer
{
	void Render(IReadOnlyList<ExtraTurn> extraTurns);
}
