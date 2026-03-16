using System;
using UnityEngine;

public class MatchmakingIntro : MonoBehaviour
{
	public Animation cameraAnim;

	private Action onComplete;

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void PlayCameraAnim(Action onComplete)
	{
		this.onComplete = onComplete;
		cameraAnim.Play();
	}

	public void Update()
	{
		if (!cameraAnim.isPlaying && onComplete != null)
		{
			onComplete();
		}
	}
}
