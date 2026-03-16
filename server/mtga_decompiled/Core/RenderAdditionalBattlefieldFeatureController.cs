using UnityEngine;
using Wotc.Mtga.Quality;

public class RenderAdditionalBattlefieldFeatureController : MonoBehaviour
{
	private void Start()
	{
		QualitySettingsUtil.Instance.SetHybridBattlefieldEnabled(enabled: true);
	}

	private void OnDestroy()
	{
		QualitySettingsUtil.Instance.SetHybridBattlefieldEnabled(enabled: false);
	}

	private void OnApplicationQuit()
	{
		QualitySettingsUtil.Instance.SetHybridBattlefieldEnabled(enabled: false);
	}
}
