using System.Collections.Generic;
using UnityEngine;

public class EndpointSelectorSwitcher : SelectorSwitcher
{
	[SerializeField]
	private List<SelectorVariants> endpointSelectors;

	private void Awake()
	{
		InstantiateVariant(endpointSelectors);
	}
}
