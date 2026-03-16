using UnityEngine;

namespace Wotc.Mtga.Wrapper.BonusPack;

public class PackProgressMeterReset : StateMachineBehaviour
{
	private PackProgressMeter _packProgressMeter;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_packProgressMeter == null)
		{
			_packProgressMeter = animator.GetComponent<PackProgressMeter>();
		}
		if (!(_packProgressMeter == null))
		{
			_packProgressMeter.ResetParametersAfterAnimation();
		}
	}
}
