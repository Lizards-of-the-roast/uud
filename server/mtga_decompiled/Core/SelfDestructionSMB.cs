using UnityEngine;

public class SelfDestructionSMB : SMBehaviour
{
	[SerializeField]
	private bool _destroySelf;

	[SerializeField]
	private bool _destroyChildren;

	protected override void OnEnter()
	{
		if (_destroySelf)
		{
			Object.Destroy(Animator.gameObject);
		}
		if (!_destroyChildren)
		{
			return;
		}
		foreach (Transform item in Animator.transform)
		{
			Object.Destroy(item.gameObject);
		}
	}

	protected override void OnUpdate()
	{
		if (_destroySelf)
		{
			Object.Destroy(Animator.gameObject);
		}
		if (!_destroyChildren)
		{
			return;
		}
		foreach (Transform item in Animator.transform)
		{
			Object.Destroy(item.gameObject);
		}
	}
}
