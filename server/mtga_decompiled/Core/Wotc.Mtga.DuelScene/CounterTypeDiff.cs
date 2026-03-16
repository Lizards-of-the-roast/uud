using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public static class CounterTypeDiff
{
	public static void GetOutput(ref CounterTypeDiffOutput output, CounterTypeDiffInput input)
	{
		output.Added.Clear();
		output.Incremented.Clear();
		output.NoChange.Clear();
		output.Decremented.Clear();
		output.Removed.Clear();
		output.Active.Clear();
		IReadOnlyDictionary<CounterType, int> previous = input.Previous;
		IReadOnlyDictionary<CounterType, int> current = input.Current;
		foreach (CounterType key in previous.Keys)
		{
			if (!current.ContainsKey(key))
			{
				output.Removed.Add(key);
			}
		}
		foreach (CounterType key2 in current.Keys)
		{
			output.Active.Add(key2);
			if (previous.TryGetValue(key2, out var value))
			{
				int num = current[key2];
				((num > value) ? output.Incremented : ((num < value) ? output.Decremented : output.NoChange)).Add(key2);
			}
			else
			{
				output.Added.Add(key2);
			}
		}
	}
}
