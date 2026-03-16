using UnityEngine;

public class AnimationForwarder_SealedBoosterOpen : MonoBehaviour
{
	public SealedBoosterOpenAnimation _controller;

	public void BeginOpenAnimation()
	{
		_controller.BeginOpenAnimation();
		AudioManager.PlayAudio("sfx_ui_sealdanimation_packopening", base.gameObject);
	}

	public void StartRevealingCards()
	{
		_controller.StartBoosterOpenAnimationSequence();
	}
}
