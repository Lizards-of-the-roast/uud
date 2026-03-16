using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class HangerConfigProviderAggregate : IHangerConfigProvider
{
	private readonly IReadOnlyList<IHangerConfigProvider> _elements;

	public HangerConfigProviderAggregate(params IHangerConfigProvider[] elements)
	{
		_elements = new List<IHangerConfigProvider>(elements);
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		foreach (IHangerConfigProvider element in _elements)
		{
			foreach (HangerConfig hangerConfig in element.GetHangerConfigs(model))
			{
				yield return hangerConfig;
			}
		}
	}
}
