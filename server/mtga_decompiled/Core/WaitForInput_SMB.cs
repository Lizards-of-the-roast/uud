using UnityEngine;
using Wotc.Mtga.CustomInput;

public class WaitForInput_SMB : StateMachineBehaviour
{
	[MinMaxSlider(0f, 1f)]
	[SerializeField]
	private Vector2 _range = new Vector2(0f, 1f);

	[SerializeField]
	private string[] _triggers;

	[SerializeField]
	private string[] _trueStates;

	[SerializeField]
	private string[] _falseStates;

	private bool _triggered;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		float num = Mathf.Clamp01(stateInfo.normalizedTime);
		if (_range.x <= num && _range.y >= num && CustomInputModule.IsAnyInputPressed() && !_triggered)
		{
			_triggered = true;
			string[] triggers = _triggers;
			foreach (string trigger in triggers)
			{
				animator.SetTrigger(trigger);
			}
			triggers = _trueStates;
			foreach (string text in triggers)
			{
				animator.SetBool(text, value: true);
			}
			triggers = _falseStates;
			foreach (string text2 in triggers)
			{
				animator.SetBool(text2, value: false);
			}
		}
	}
}
