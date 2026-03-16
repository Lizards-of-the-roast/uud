using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnvironmentSelector : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown _environment_select_dropdown;

	private List<string> _environments;

	public event Action<string> NewEnvironmentSelected;

	public void SetEnvironments(List<string> environments, string current)
	{
		_environments = environments;
		List<TMP_Dropdown.OptionData> options = environments.Select((string s) => new TMP_Dropdown.OptionData(s)).ToList();
		_environment_select_dropdown.options = options;
		int value = _environment_select_dropdown.options.FindIndex((TMP_Dropdown.OptionData x) => x.text == current);
		_environment_select_dropdown.value = value;
		_environment_select_dropdown.onValueChanged.AddListener(HandleServerDropdownChange);
	}

	public void ShowEnvironmentDropdown(bool value)
	{
		bool active = MDNPlayerPrefs.DisplayEnvironmentAndBundleEndpointSelectors && value;
		_environment_select_dropdown.gameObject.SetActive(active);
	}

	private void HandleServerDropdownChange(int newVal)
	{
		string serverName = _environment_select_dropdown.options[newVal].text;
		string obj = _environments.Find((string x) => x == serverName);
		this.NewEnvironmentSelected?.Invoke(obj);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
		{
			ShowEnvironmentDropdown(!_environment_select_dropdown.gameObject.activeInHierarchy);
		}
	}
}
