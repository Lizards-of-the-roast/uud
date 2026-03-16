using UnityEngine;

public class PlayAudio : MonoBehaviour
{
	[SerializeField]
	private bool muted;

	public void PlayAudioEvent(string audioEvent)
	{
		if (!muted && !string.IsNullOrEmpty(audioEvent))
		{
			AudioManager.PlayAudio(audioEvent, base.gameObject);
		}
	}
}
