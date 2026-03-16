using UnityEngine;

public class AnimationForwarder_BoosterChamber : MonoBehaviour
{
	[SerializeField]
	public BoosterChamberController _boosterChamberController;

	public void StartRevealingCards()
	{
		_boosterChamberController.StartBoosterOpenAnimationSequence();
	}

	public void Anm_RefreshSetLogo()
	{
		_boosterChamberController.RefreshLogoAndBooster();
	}

	public void Anm_SetLogoOut()
	{
		_boosterChamberController.SetLogoOut();
	}
}
