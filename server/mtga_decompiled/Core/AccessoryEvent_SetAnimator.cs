using UnityEngine;

[CreateAssetMenu(fileName = "AE_SA_New", menuName = "ScriptableObject/AccessoryEvents/SetAnimator", order = 1)]
public class AccessoryEvent_SetAnimator : AccessoryEventSO
{
	[SerializeField]
	private string _triggerName;

	[SerializeField]
	private string _boolName;

	[SerializeField]
	private bool _boolValue;

	public void Execute(Animator animator)
	{
		if (!string.IsNullOrEmpty(_triggerName))
		{
			animator.SetTrigger(_triggerName);
		}
		if (!string.IsNullOrEmpty(_boolName))
		{
			animator.SetBool(_boolName, _boolValue);
		}
	}
}
