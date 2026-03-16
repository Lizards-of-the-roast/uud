using UnityEngine;

namespace Core.Meta.Emotes;

public class SpriteFadeGroupControlStateMachineBehaviour : StateMachineBehaviour
{
	[SerializeField]
	private SpriteFadeGroup.FadeState _desiredFadeState;

	private SpriteFadeGroup _spriteFadeGroup;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_spriteFadeGroup = animator.GetComponent<SpriteFadeGroup>();
		if (!(_spriteFadeGroup == null))
		{
			_spriteFadeGroup.CurrentFadeState = _desiredFadeState;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_spriteFadeGroup != null && _desiredFadeState == SpriteFadeGroup.FadeState.FadingIn)
		{
			_spriteFadeGroup.CurrentFadeState = SpriteFadeGroup.FadeState.Inactive;
		}
	}
}
