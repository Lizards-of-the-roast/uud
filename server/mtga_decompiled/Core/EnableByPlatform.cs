using UnityEngine;
using Wizards.Mtga.Platforms;

public class EnableByPlatform : MonoBehaviour
{
	[SerializeField]
	private DeviceType _platformToEnable;

	[SerializeField]
	private bool _enableOnHandheld4x3 = true;

	private void OnEnable()
	{
		if (PlatformUtils.GetCurrentDeviceType() == _platformToEnable)
		{
			if (PlatformUtils.IsHandheld() && !_enableOnHandheld4x3 && PlatformUtils.IsAspectRatio4x3())
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				base.gameObject.SetActive(value: true);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
