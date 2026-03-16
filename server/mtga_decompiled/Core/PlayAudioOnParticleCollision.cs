using UnityEngine;

public class PlayAudioOnParticleCollision : MonoBehaviour
{
	[SerializeField]
	private string audioEvent;

	private void OnParticleCollision(GameObject other)
	{
		if (!string.IsNullOrEmpty(audioEvent))
		{
			AudioManager.PlayAudio(audioEvent, base.gameObject);
		}
	}
}
