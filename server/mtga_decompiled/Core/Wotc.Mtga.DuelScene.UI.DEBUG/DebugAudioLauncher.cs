using UnityEngine;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class DebugAudioLauncher : MonoBehaviour
{
	[SerializeField]
	private GameObject _wwiseGlobalListener;

	[SerializeField]
	private GameObject _mobileWwiseGlobalListener;

	private void Awake()
	{
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android)
		{
			Object.Instantiate(_mobileWwiseGlobalListener);
		}
		else
		{
			Object.Instantiate(_wwiseGlobalListener);
		}
	}
}
