using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.DuelScene.UXEvents;

public class BattlefieldManager : MonoBehaviour
{
	public static WindEvent battlefieldWind;

	public static List<ParticleSystem> windTriggeredParticles = new List<ParticleSystem>();

	public static List<PlayAudioBFTrigger> triggeredSounds = new List<PlayAudioBFTrigger>();

	public static List<Light> dimmableLights = new List<Light>();

	public static List<float> dimmableLightDefaultIntensities = new List<float>();

	public static List<Material> dimmableMaterials = new List<Material>();

	private static CritterManager _critterManager;

	[SerializeField]
	private float _cameraShakeMinMagnitude;

	[SerializeField]
	private float _cameraShakeMaxMagnitude = 10f;

	[SerializeField]
	private float _shakeScale = 1f;

	[SerializeField]
	private float _cameraShakeDurationMax = 0.5f;

	private bool _cameraIsShaking;

	public static CritterManager BattlefieldCritterManager
	{
		get
		{
			if (null == _critterManager)
			{
				_critterManager = Object.FindObjectOfType<CritterManager>();
			}
			return _critterManager;
		}
	}

	public static void EmitWindParticles(float percentageEmission)
	{
		if (windTriggeredParticles == null || windTriggeredParticles.Count == 0)
		{
			return;
		}
		int num = 100;
		int count = (int)(percentageEmission * (float)num);
		for (int i = 0; i < windTriggeredParticles.Count; i++)
		{
			if (windTriggeredParticles[i] != null)
			{
				windTriggeredParticles[i].Emit(count);
			}
		}
	}

	public static void TriggerSounds()
	{
		for (int i = 0; i < triggeredSounds.Count; i++)
		{
			triggeredSounds[i].TriggerWwiseEvent();
		}
	}

	private void OnDestroy()
	{
		_critterManager = null;
		dimmableLights.Clear();
		dimmableLightDefaultIntensities.Clear();
		dimmableMaterials.Clear();
		windTriggeredParticles.Clear();
		triggeredSounds.Clear();
		if (battlefieldWind != null)
		{
			Object.Destroy(battlefieldWind.gameObject);
			battlefieldWind = null;
		}
		AssetLoader.ReleaseAsset(BattlefieldUtil.BattlefieldPath);
	}

	public void PlayBattlefieldWind()
	{
		if (battlefieldWind != null)
		{
			battlefieldWind.Play();
		}
	}

	public void ShakeCamera(int amount)
	{
		if (!_cameraIsShaking && amount != 0)
		{
			StartCoroutine(CR_CameraShake(amount));
		}
	}

	public void InitBattlefieldReactions(UXEventQueue eventQueue)
	{
		BattlefieldPhenomenonReactions[] array = Object.FindObjectsOfType<BattlefieldPhenomenonReactions>();
		if (array != null)
		{
			BattlefieldPhenomenonReactions[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Init(eventQueue);
			}
		}
	}

	private IEnumerator CR_CameraShake(int amount)
	{
		if (CurrentCamera.Value == null)
		{
			yield break;
		}
		Transform cameraTransform = CurrentCamera.Value.transform;
		if (!(cameraTransform == null))
		{
			float num = Mathf.Clamp(amount, _cameraShakeMinMagnitude, _cameraShakeMaxMagnitude);
			float _shakeDuration = num / _cameraShakeMaxMagnitude * _cameraShakeDurationMax;
			float _cameraShakeMagnitude = num * _shakeScale;
			float cameraShakeElapsed = 0f;
			_cameraIsShaking = true;
			Vector3 originalCamPos = cameraTransform.localPosition;
			while (cameraShakeElapsed < _shakeDuration)
			{
				cameraShakeElapsed += Time.deltaTime;
				float num2 = cameraShakeElapsed / _shakeDuration;
				float num3 = 1f - Mathf.Clamp(4f * num2 - 3f, 0f, 1f);
				float num4 = Random.value * 2f - 1f;
				float num5 = Random.value * 2f - 1f;
				num4 *= _cameraShakeMagnitude * num3;
				num5 *= _cameraShakeMagnitude * num3;
				cameraTransform.localPosition = new Vector3(originalCamPos.x + num4, originalCamPos.y + num5, originalCamPos.z);
				yield return null;
			}
			_cameraIsShaking = false;
			cameraTransform.localPosition = originalCamPos;
		}
	}
}
