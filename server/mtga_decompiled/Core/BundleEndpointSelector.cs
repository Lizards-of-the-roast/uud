using System;
using System.Collections.Generic;
using Assets.Core.Code.AssetBundles;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Assets;

public class BundleEndpointSelector : MonoBehaviour
{
	[SerializeField]
	private Dropdown _endpointDropdown;

	private AssetBundleSourcesModel _sourcesModel;

	public event Action<IAssetBundleSource> NewBundleSourceSelected;

	public void SetSources(AssetBundleSourcesModel sourcesModel)
	{
		_sourcesModel = sourcesModel;
		List<string> list = new List<string>();
		foreach (IAssetBundleSource source in sourcesModel.Sources)
		{
			list.Add(source.ToString());
		}
		_endpointDropdown.AddOptions(list);
		int currentSourceIndex = sourcesModel.CurrentSourceIndex;
		if (currentSourceIndex >= 0)
		{
			_endpointDropdown.value = currentSourceIndex;
		}
		_endpointDropdown.onValueChanged.AddListener(OnDropDownValueChanged);
		ShowEndpointDropdown(sourcesModel.HasMultipleOptionsToSelect);
	}

	public void ShowEndpointDropdown(bool value)
	{
		bool active = MDNPlayerPrefs.DisplayEnvironmentAndBundleEndpointSelectors && _sourcesModel.HasMultipleOptionsToSelect && value;
		_endpointDropdown.gameObject.SetActive(active);
	}

	private void OnDropDownValueChanged(int newVal)
	{
		_sourcesModel.CurrentSourceIndex = newVal;
		this.NewBundleSourceSelected?.Invoke(_sourcesModel.CurrentSource);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
		{
			ShowEndpointDropdown(!_endpointDropdown.gameObject.activeInHierarchy);
		}
	}
}
