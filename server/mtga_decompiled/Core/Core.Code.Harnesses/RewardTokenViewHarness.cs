using System;
using Core.Meta.MainNavigation.Rewards;
using Core.Meta.Tokens;
using UnityEngine;
using Wizards.Mtga;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class RewardTokenViewHarness : Harness<TokenRewardView, HarnessDropdownUI>
{
	[Serializable]
	public class NavBarViewModel
	{
		public string Description;

		public string HeaderLocKey;

		public string DescriptionLocKey;

		public int Quantity;

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
		HarnessCore.CenterView(_Instance.GetComponent<RectTransform>());
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		NavBarViewModel navBarViewModel = _viewModels[newValue];
		string tokenLocalizationKey = TokenViewUtilities.GetTokenLocalizationKey(null, navBarViewModel.HeaderLocKey, navBarViewModel.Quantity);
		_Instance.Refresh(tokenLocalizationKey, navBarViewModel.Quantity, navBarViewModel.DescriptionLocKey);
	}
}
