using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.DuelScene.VFX;

public class SelfCleanup : MonoBehaviour
{
	public enum CleanupType
	{
		Destroy,
		SharedPool,
		DuelScenePool
	}

	[SerializeField]
	private CleanupType _type;

	[SerializeField]
	private float _lifetime = 3f;

	[SerializeField]
	private bool _onlyWhenChildless;

	private float _remainingLife;

	public void SetLifetime(float newLifetime, CleanupType? cleanupType = null, bool onlyWhenChildless = false)
	{
		_lifetime = newLifetime;
		_remainingLife = newLifetime;
		_onlyWhenChildless = onlyWhenChildless;
		if (cleanupType.HasValue)
		{
			_type = cleanupType.Value;
		}
	}

	private void OnEnable()
	{
		_remainingLife = _lifetime;
	}

	public void ManualCleanup()
	{
		_remainingLife = 0f;
	}

	public void ImmediateCleanup()
	{
		LoopingAnimationManager.RemoveLoopingEffect(base.gameObject);
		if (_type == CleanupType.SharedPool || _type == CleanupType.DuelScenePool)
		{
			IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
			if (unityObjectPool != null)
			{
				unityObjectPool.PushObject(base.gameObject);
				return;
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void LateUpdate()
	{
		_remainingLife -= Time.deltaTime;
		if (_remainingLife <= 0f && (!_onlyWhenChildless || base.transform.childCount <= 0))
		{
			ImmediateCleanup();
		}
	}
}
