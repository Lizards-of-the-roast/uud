using System.Collections;
using Pooling;
using UnityEngine;
using Wizards.Mtga;

public class TimeoutPip : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	private IUnityObjectPool _objectPool;

	private void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		_animator.keepAnimatorStateOnDisable = true;
	}

	public void TimeoutUse()
	{
		_animator.SetTrigger("TimeoutUse");
		StartCoroutine(CleanUp());
	}

	private IEnumerator CleanUp()
	{
		yield return new WaitForSeconds(2.5f);
		_animator.SetTrigger("TESTRESET");
		_objectPool.PushObject(base.gameObject);
	}
}
