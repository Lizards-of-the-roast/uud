using UnityEngine;

public class PlayOnIsVisible : MonoBehaviour
{
	public string AudioEvent = string.Empty;

	private Renderer _renderer;

	private bool soundPlayed;

	private void Start()
	{
		_renderer = base.gameObject.GetComponent<Renderer>();
	}

	private void Update()
	{
		if (!(_renderer != null))
		{
			return;
		}
		if (_renderer.isVisible)
		{
			if (!soundPlayed)
			{
				AudioManager.PlayAudio(AudioEvent, base.gameObject);
				soundPlayed = true;
			}
		}
		else
		{
			soundPlayed = false;
		}
	}
}
