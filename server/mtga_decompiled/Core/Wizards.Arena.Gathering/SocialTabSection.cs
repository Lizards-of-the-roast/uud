using System;
using UnityEngine;
using UnityEngine.UI;

namespace Wizards.Arena.Gathering;

[Serializable]
public class SocialTabSection
{
	[SerializeField]
	private string _sectionName;

	[SerializeField]
	private Button _sectionButton;

	[SerializeField]
	private GameObject _sectionPrefab;

	[SerializeField]
	private Transform _sectionParent;

	public string SectionName => _sectionName;

	public Button SectionButton => _sectionButton;

	public GameObject Load()
	{
		if (_sectionPrefab == null)
		{
			Debug.LogError("There was no section prefab assigned.");
			return null;
		}
		if (_sectionParent == null)
		{
			Debug.LogError("There was no section parent assigned.");
			return null;
		}
		return UnityEngine.Object.Instantiate(_sectionPrefab, _sectionParent);
	}
}
