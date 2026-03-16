using System;
using System.Collections.Generic;
using Core.Code.Collations;
using Core.Code.Collections;
using Core.Meta.UI;
using Core.Shared.Code.CardFilters;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class AdvancedFiltersViewHarness : Harness<AdvancedFiltersView, HarnessDropdownUI>
{
	[Serializable]
	public class AdvancedFiltersViewModel
	{
		public string Description;

		public List<ClientSetMetadata> SetDatas;

		public List<ClientSetGroup> SetGroups;

		public override string ToString()
		{
			return Description;
		}
	}

	[SerializeField]
	private AdvancedFiltersViewModel[] _viewModels;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
		TooltipTrigger.Inject(Pantry.Get<TooltipSystem>());
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		SceneUITransforms sceneUITransforms = Pantry.Get<SceneUITransforms>();
		_Instance.transform.SetParent(sceneUITransforms.PopupsParent, worldPositionStays: false);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		AdvancedFiltersViewModel viewModel = _viewModels[newValue];
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		setMetadataProvider.LoadData(ToSetMetadataCollection(viewModel));
		CardFilter model = new CardFilter(cardDatabase, cardDatabase.GreLocProvider, setMetadataProvider);
		_Instance.SetModel(model);
	}

	public static SetMetadataCollection ToSetMetadataCollection(AdvancedFiltersViewModel viewModel)
	{
		return new SetMetadataCollection
		{
			SetDatas = viewModel.SetDatas,
			SetGroups = viewModel.SetGroups,
			StoreSetGroups = new List<ClientStorePackGroup>()
		};
	}
}
