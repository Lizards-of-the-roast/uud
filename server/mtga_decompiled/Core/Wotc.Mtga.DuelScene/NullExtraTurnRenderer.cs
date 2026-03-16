using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullExtraTurnRenderer : IExtraTurnRenderer
{
	public static readonly IExtraTurnRenderer Default = new NullExtraTurnRenderer();

	public void Render(IReadOnlyList<ExtraTurn> extraTurns)
	{
	}
}
