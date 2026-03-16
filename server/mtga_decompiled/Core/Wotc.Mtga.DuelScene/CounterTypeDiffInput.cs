using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public readonly struct CounterTypeDiffInput
{
	public readonly IReadOnlyDictionary<CounterType, int> Previous;

	public readonly IReadOnlyDictionary<CounterType, int> Current;

	public CounterTypeDiffInput(IReadOnlyDictionary<CounterType, int> previous, IReadOnlyDictionary<CounterType, int> current)
	{
		Previous = previous;
		Current = current;
	}
}
