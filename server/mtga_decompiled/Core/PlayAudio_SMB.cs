using UnityEngine;

public class PlayAudio_SMB : StateMachineBehaviour
{
	[SerializeField]
	private string EnterAudio;

	[SerializeField]
	private float EnterAudioDelay;

	[SerializeField]
	private string ExitAudio;

	[SerializeField]
	private float ExitAudioDelay;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(EnterAudio))
		{
			AudioManager.PlayAudio(EnterAudio, animator.gameObject, EnterAudioDelay);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(ExitAudio))
		{
			AudioManager.PlayAudio(ExitAudio, animator.gameObject, ExitAudioDelay);
		}
	}
}
