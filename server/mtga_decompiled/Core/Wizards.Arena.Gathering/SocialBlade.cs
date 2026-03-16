using System;
using UnityEngine;

namespace Wizards.Arena.Gathering;

public class SocialBlade : MonoBehaviour
{
	[SerializeField]
	private SocialTabSection[] _sections;

	private GameObject _activeSectionObject;

	private SocialTabSection _activeSection;

	private void Start()
	{
		SocialTabSection[] sections = _sections;
		foreach (SocialTabSection curSection in sections)
		{
			curSection.SectionButton.onClick.AddListener(delegate
			{
				if (_activeSectionObject != null)
				{
					if (_activeSection != null && _activeSection.SectionName.Equals(curSection.SectionName, StringComparison.InvariantCulture))
					{
						return;
					}
					UnityEngine.Object.Destroy(_activeSectionObject);
				}
				_activeSection = curSection;
				_activeSectionObject = curSection.Load();
			});
		}
	}
}
