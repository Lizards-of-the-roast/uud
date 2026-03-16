using Core.Meta.NewPlayerExperience.Graph;
using UnityEngine;
using Wizards.Mtga;

public class BannerEventsUnlockedSMB : StateMachineBehaviour
{
	private readonly int _unlockedParameterHash = Animator.StringToHash("BannerEventsUnlocked");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool(_unlockedParameterHash, Pantry.Get<NewPlayerExperienceStrategy>().OpenedDualColorPreconEvent.Result);
	}
}
