using System;
using System.Collections.Generic;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation.NavBar;
using Core.Shared.Code.ClientModels;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class NavBarTokenViewHarness : Harness<NavBarTokenView, HarnessDropdownUI>
{
	[Serializable]
	public class NavBarViewModel
	{
		public string Description;

		public List<Client_CustomTokenDefinitionWithQty> tokens;

		public override string ToString()
		{
			return Description;
		}
	}

	[SerializeField]
	private NavBarViewModel[] _viewModels;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
		TooltipTrigger.Inject(Pantry.Get<TooltipSystem>());
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		_Instance.Init(Pantry.Get<IClientLocProvider>(), Pantry.Get<AssetLookupManager>().AssetLookupSystem);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		NavBarViewModel navBarViewModel = _viewModels[newValue];
		_Instance.UpdateTokensTooltip(navBarViewModel.tokens);
	}
}
