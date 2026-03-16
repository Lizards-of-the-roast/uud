using Pooling;
using UnityEngine;
using Wizards.Mtga;

namespace Wotc.Mtga.VFX;

public class VFXPrefabPlayer : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _particlePrefab;

	[Tooltip("Destroys instance after this amount of time, resets after each Play call")]
	[SerializeField]
	private float _destroyDelay = 10f;

	private float _destroyTimer;

	private ParticleSystem _particleInstance;

	public void Play()
	{
		if (!_particleInstance)
		{
			SpawnInstance();
		}
		if ((bool)_particleInstance)
		{
			_particleInstance.Play();
			_destroyTimer = _destroyDelay;
		}
	}

	public void Stop()
	{
		if ((bool)_particleInstance)
		{
			_particleInstance.Stop();
		}
	}

	public void Clear()
	{
		if ((bool)_particleInstance)
		{
			_particleInstance.Clear();
		}
	}

	private void SpawnInstance()
	{
		_ = (bool)_particlePrefab;
		IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
		if (unityObjectPool != null)
		{
			GameObject gameObject = unityObjectPool.PopObject(_particlePrefab.gameObject);
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			_particleInstance = gameObject.GetComponent<ParticleSystem>();
		}
	}

	public void ImmediateCleanup()
	{
		if ((bool)_particleInstance)
		{
			Pantry.Get<IUnityObjectPool>()?.PushObject(_particleInstance.gameObject);
			_particleInstance = null;
		}
	}

	private void LateUpdate()
	{
		if ((bool)_particleInstance)
		{
			_destroyTimer -= Time.deltaTime;
			if (_destroyTimer <= 0f)
			{
				ImmediateCleanup();
			}
		}
	}
}
