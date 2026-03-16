using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public readonly struct CounterTypeDiffOutput
{
	public readonly HashSet<CounterType> Added;

	public readonly HashSet<CounterType> Incremented;

	public readonly HashSet<CounterType> NoChange;

	public readonly HashSet<CounterType> Decremented;

	public readonly HashSet<CounterType> Removed;

	public readonly List<CounterType> Active;

	public static CounterTypeDiffOutput empty => new CounterTypeDiffOutput(new HashSet<CounterType>(), new HashSet<CounterType>(), new HashSet<CounterType>(), new HashSet<CounterType>(), new HashSet<CounterType>(), new List<CounterType>());

	public CounterTypeDiffOutput(HashSet<CounterType> added, HashSet<CounterType> incremented, HashSet<CounterType> noChange, HashSet<CounterType> decremented, HashSet<CounterType> removed, List<CounterType> active)
	{
		Added = added ?? new HashSet<CounterType>();
		Incremented = incremented ?? new HashSet<CounterType>();
		NoChange = noChange ?? new HashSet<CounterType>();
		Decremented = decremented ?? new HashSet<CounterType>();
		Removed = removed ?? new HashSet<CounterType>();
		Active = active ?? new List<CounterType>();
	}
}
