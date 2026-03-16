using System.Collections;
using Core.Shared.Code.Utilities;
using UnityEngine;

public class LightEvent : MonoBehaviour
{
	public float eventLength = 1f;

	public float strengthModifier = 1f;

	public AnimationCurve lightIntensityMultiplier;

	private Coroutine _hitTheLightsRoutine;

	public void Play()
	{
		Stop();
		_hitTheLightsRoutine = StartCoroutine(HitTheLights());
	}

	public void Stop()
	{
		if (_hitTheLightsRoutine != null)
		{
			StopCoroutine(_hitTheLightsRoutine);
			_hitTheLightsRoutine = null;
		}
		ResetLights();
	}

	private void OnEnable()
	{
		Play();
	}

	private void OnDisable()
	{
		Stop();
	}

	private IEnumerator HitTheLights()
	{
		float playTime = 0f;
		while (playTime < eventLength)
		{
			playTime += Time.deltaTime;
			float num = strengthModifier * lightIntensityMultiplier.Evaluate(playTime / eventLength);
			for (int i = 0; i < BattlefieldManager.dimmableLights.Count; i++)
			{
				BattlefieldManager.dimmableLights[i].intensity = BattlefieldManager.dimmableLightDefaultIntensities[i] * num;
			}
			for (int j = 0; j < BattlefieldManager.dimmableMaterials.Count; j++)
			{
				BattlefieldManager.dimmableMaterials[j].SetFloat(ShaderPropertyIds.BrightnessPropId, Mathf.Clamp(num, 0f, 1f));
			}
			yield return null;
		}
		ResetLights();
		yield return null;
	}

	private void ResetLights()
	{
		for (int i = 0; i < BattlefieldManager.dimmableLights.Count; i++)
		{
			BattlefieldManager.dimmableLights[i].intensity = BattlefieldManager.dimmableLightDefaultIntensities[i];
		}
		for (int j = 0; j < BattlefieldManager.dimmableMaterials.Count; j++)
		{
			BattlefieldManager.dimmableMaterials[j].SetFloat(ShaderPropertyIds.BrightnessPropId, 1f);
		}
	}
}
