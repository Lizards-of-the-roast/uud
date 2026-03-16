using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SelectManaTypeBrowser : BrowserBase
{
	private CastingTimeOption_ManaTypeWorkflow _workflow;

	private readonly Dictionary<ManaTypeSpinner, CastingTimeOption_ManaTypeWorkflow.SelectionPair> _spinners = new Dictionary<ManaTypeSpinner, CastingTimeOption_ManaTypeWorkflow.SelectionPair>(5);

	public SelectManaTypeBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider browserProvider, GameManager gameManager)
		: base(browserManager, browserProvider, gameManager)
	{
		_workflow = browserProvider as CastingTimeOption_ManaTypeWorkflow;
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		HorizontalLayoutGroup componentInChildren = GetBrowserElement("SpinnerContainer").GetComponentInChildren<HorizontalLayoutGroup>();
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CardBrowserType = _workflow.GetBrowserType();
		_assetLookupSystem.Blackboard.DeviceType = PlatformUtils.GetCurrentDeviceType();
		_assetLookupSystem.Blackboard.AspectRatio = (float)Screen.width / (float)Screen.height;
		_assetLookupSystem.Blackboard.CardBrowserElementID = "ManaTypeSpinner";
		BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		if (_workflow.CastingCost.Count > 0)
		{
			int num = 0;
			for (int i = 0; i < _workflow.CastingCost.Count; i++)
			{
				CastingTimeOption_ManaTypeWorkflow.SelectionPair selectionPair = null;
				ManaTypeSpinner component = AssetLoader.Instantiate(payload.Prefab, componentInChildren.transform).GetComponent<ManaTypeSpinner>();
				ManaQuantity option = _workflow.CastingCost[i];
				if (option.Hybrid)
				{
					selectionPair = _workflow.SelectionPairs[num];
					num++;
					component.Init(selectionPair);
					component.SetUpArrowInteractable(selectionPair.SelectedIndex > 0);
					component.SetDownArrowInteractable(selectionPair.SelectedIndex < selectionPair.Options.Count - 1);
					component.UpEvent += OnSpinnerUpClicked;
					component.DownEvent += OnSpinnerDownClicked;
				}
				else
				{
					component.Init(option);
				}
				_spinners.Add(component, selectionPair);
			}
		}
		else
		{
			foreach (CastingTimeOption_ManaTypeWorkflow.SelectionPair selectionPair2 in _workflow.SelectionPairs)
			{
				ManaTypeSpinner component2 = AssetLoader.Instantiate(payload.Prefab, componentInChildren.transform).GetComponent<ManaTypeSpinner>();
				component2.Init(selectionPair2);
				component2.SetUpArrowInteractable(selectionPair2.SelectedIndex > 0);
				component2.SetDownArrowInteractable(selectionPair2.SelectedIndex < selectionPair2.Options.Count - 1);
				component2.UpEvent += OnSpinnerUpClicked;
				component2.DownEvent += OnSpinnerDownClicked;
				_spinners.Add(component2, selectionPair2);
			}
		}
		BrowserHeader component3 = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component3.SetHeaderText(_workflow.GetHeaderText());
		component3.SetSubheaderText(_workflow.GetSubHeaderText());
	}

	protected override void ReleaseUIElements()
	{
		foreach (KeyValuePair<ManaTypeSpinner, CastingTimeOption_ManaTypeWorkflow.SelectionPair> spinner in _spinners)
		{
			spinner.Key.UpEvent -= OnSpinnerUpClicked;
			spinner.Key.DownEvent -= OnSpinnerDownClicked;
			spinner.Key.Cleanup();
			Object.Destroy(spinner.Key.gameObject);
		}
		_spinners.Clear();
		base.ReleaseUIElements();
	}

	private void OnSpinnerUpClicked(ManaTypeSpinner spinner)
	{
		CastingTimeOption_ManaTypeWorkflow.SelectionPair selectionPair = _spinners[spinner];
		if (selectionPair.SelectedIndex > 0)
		{
			selectionPair.SelectedIndex--;
		}
		spinner.SetIndex(selectionPair.SelectedIndex);
		spinner.SetUpArrowInteractable(selectionPair.SelectedIndex > 0);
		spinner.SetDownArrowInteractable(selectionPair.SelectedIndex < selectionPair.Options.Count - 1);
	}

	private void OnSpinnerDownClicked(ManaTypeSpinner spinner)
	{
		CastingTimeOption_ManaTypeWorkflow.SelectionPair selectionPair = _spinners[spinner];
		if (selectionPair.SelectedIndex < selectionPair.Options.Count - 1)
		{
			selectionPair.SelectedIndex++;
		}
		spinner.SetIndex(selectionPair.SelectedIndex);
		spinner.SetUpArrowInteractable(selectionPair.SelectedIndex > 0);
		spinner.SetDownArrowInteractable(selectionPair.SelectedIndex < selectionPair.Options.Count - 1);
	}
}
