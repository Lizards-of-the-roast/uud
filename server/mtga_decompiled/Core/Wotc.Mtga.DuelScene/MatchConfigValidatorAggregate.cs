using System;
using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class MatchConfigValidatorAggregate : IMatchConfigValidator
{
	private readonly IMatchConfigValidator[] _elements;

	private readonly HashSet<(IMatchConfigValidator.Result, string)> _results = new HashSet<(IMatchConfigValidator.Result, string)>();

	public MatchConfigValidatorAggregate(params IMatchConfigValidator[] elements)
	{
		_elements = elements ?? Array.Empty<IMatchConfigValidator>();
	}

	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		_results.Clear();
		IMatchConfigValidator[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			foreach (var result in elements[i].GetResults(matchConfig))
			{
				_results.Add(result);
			}
		}
		return _results;
	}
}
