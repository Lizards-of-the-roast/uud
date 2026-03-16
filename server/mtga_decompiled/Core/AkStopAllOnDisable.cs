using UnityEngine;

public class AkStopAllOnDisable : MonoBehaviour
{
	public void OnDisable()
	{
		AkSoundEngine.StopAll(base.gameObject);
	}

	private void OnDestroy()
	{
		AkSoundEngine.StopAll(base.gameObject);
	}
}
