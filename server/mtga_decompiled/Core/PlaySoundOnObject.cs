using UnityEngine;

public class PlaySoundOnObject : MonoBehaviour
{
	[SerializeField]
	private string onAwake;

	[SerializeField]
	private string onDestroy;

	[SerializeField]
	private bool useEnableDisable;

	private void Awake()
	{
		if (!useEnableDisable && !string.IsNullOrEmpty(onAwake))
		{
			AudioManager.PlayAudio(onAwake, base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (!useEnableDisable && !string.IsNullOrEmpty(onDestroy))
		{
			AudioManager.PlayAudio(onDestroy, base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (useEnableDisable && !string.IsNullOrEmpty(onAwake))
		{
			AudioManager.PlayAudio(onAwake, base.gameObject);
		}
	}

	private void OnDisable()
	{
		if (useEnableDisable && !string.IsNullOrEmpty(onDestroy))
		{
			AudioManager.PlayAudio(onDestroy, base.gameObject);
		}
	}
}
