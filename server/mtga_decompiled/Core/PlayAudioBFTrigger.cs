using UnityEngine;

public class PlayAudioBFTrigger : MonoBehaviour
{
	[SerializeField]
	private string WwiseEvent;

	public void TriggerWwiseEvent()
	{
		if (WwiseEvent != null)
		{
			AudioManager.PlayAudio(WwiseEvent, base.gameObject);
		}
	}
}
