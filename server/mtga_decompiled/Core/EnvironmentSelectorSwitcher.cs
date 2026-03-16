using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSelectorSwitcher : SelectorSwitcher
{
	[SerializeField]
	private List<SelectorVariants> enviromentSelectors;

	private void Awake()
	{
		InstantiateVariant(enviromentSelectors);
	}
}
