using System;
using System.Collections.Generic;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation.Store.Data;
using UnityEngine;
using Wizards.Mtga;
using com.wizards.harness;

namespace Core.Meta.MainNavigation.Store.Harness;

public class StoreSetFilterHarness : Harness<StoreSetFilterToggles, HarnessDropdownUI>
{
	[Serializable]
	public class StoreSetFilterViewModel
	{
		public string Description;

		public List<StoreSetFilterModel> SetFilters = new List<StoreSetFilterModel>();

		public int SelectedIndex = -1;

		public override string ToString()
		{
			return Description;
		}
	}

	[SerializeField]
	private StoreSetFilterViewModel[] _viewModels;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		_Instance.Init(Pantry.Get<AssetLookupManager>().AssetLookupSystem, null);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		StoreSetFilterViewModel storeSetFilterViewModel = _viewModels[newValue];
		_Instance.SetSetFilters(storeSetFilterViewModel.SetFilters);
		if (storeSetFilterViewModel.SelectedIndex > -1)
		{
			_Instance.Select(storeSetFilterViewModel.SelectedIndex);
		}
	}
}
