using UnityEngine;

public class AccessoryHelper_AnimCooldownTrigger : MonoBehaviour
{
	public float timeToTriggerRoost = 10f;

	private bool firstTime;

	private bool isInitialized;

	private void Start()
	{
		firstTime = true;
		isInitialized = false;
	}

	private void Update()
	{
		if (GetComponent<AccessoryController>()._ownerPlayerNum != GREPlayerNum.Opponent)
		{
			return;
		}
		Animator componentInChildren = GetComponentInChildren<Animator>();
		if (componentInChildren.GetCurrentAnimatorStateInfo(0).IsName("Idle_Flying") && firstTime)
		{
			isInitialized = true;
			if (timeToTriggerRoost > 0f)
			{
				timeToTriggerRoost -= Time.deltaTime;
			}
			else if (timeToTriggerRoost < 0f)
			{
				firstTime = false;
				ForceThisTriggerOnOppAnimator("Mouse_ClickOn", componentInChildren);
			}
		}
		if (componentInChildren.GetCurrentAnimatorStateInfo(0).IsName("Idle_Roost") && isInitialized)
		{
			firstTime = false;
		}
	}

	public static bool HasParameter(string _paramName, Animator _animator)
	{
		AnimatorControllerParameter[] parameters = _animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == _paramName)
			{
				return true;
			}
		}
		return false;
	}

	private void ForceThisTriggerOnOppAnimator(string _triggerName, Animator _anim)
	{
		if (HasParameter(_triggerName, _anim))
		{
			_anim.SetTrigger(_triggerName);
		}
	}
}
