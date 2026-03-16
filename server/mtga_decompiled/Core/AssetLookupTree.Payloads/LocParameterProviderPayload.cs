using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public abstract class LocParameterProviderPayload : IPayload
{
	public readonly List<ILocParameterProvider> ParameterProviders = new List<ILocParameterProvider>();

	public (string, string)[] BuildParameters(IBlackboard filledBB)
	{
		if (ParameterProviders.Count == 0)
		{
			return Array.Empty<(string, string)>();
		}
		List<(string, string)> list = new List<(string, string)>(ParameterProviders.Count);
		foreach (ILocParameterProvider parameterProvider in ParameterProviders)
		{
			if (parameterProvider.TryGetValue(filledBB, out var paramValue))
			{
				list.Add((parameterProvider.GetKey(), paramValue));
			}
		}
		return list.ToArray();
	}

	public virtual IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
