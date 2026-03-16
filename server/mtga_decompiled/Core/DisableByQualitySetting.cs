using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Quality;

public class DisableByQualitySetting : MonoBehaviour
{
	public int DisableOnOrBelowQualityLevel;

	private int qualitySetting;

	private void OnEnable()
	{
		if (PlatformUtils.IsHandheld())
		{
			qualitySetting = QualitySettingsUtil.Instance.GlobalQualityLevel;
			if (qualitySetting >= DisableOnOrBelowQualityLevel)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
