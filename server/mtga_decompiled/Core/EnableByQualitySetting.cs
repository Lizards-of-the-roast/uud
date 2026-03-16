using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Quality;

public class EnableByQualitySetting : MonoBehaviour
{
	public int EnableOnOrBelowQualityLevel;

	private int qualitySetting;

	private void OnEnable()
	{
		if (PlatformUtils.IsHandheld())
		{
			qualitySetting = QualitySettingsUtil.Instance.GlobalQualityLevel;
			if (qualitySetting >= EnableOnOrBelowQualityLevel)
			{
				base.gameObject.SetActive(value: true);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
