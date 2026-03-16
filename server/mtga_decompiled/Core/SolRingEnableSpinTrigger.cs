using UnityEngine;

public class SolRingEnableSpinTrigger : MonoBehaviour
{
	[SerializeField]
	private SpinTrigger spinTriggerComponent;

	public void EnableSpinTrigger()
	{
		spinTriggerComponent.EnableSpinTrigger();
	}
}
