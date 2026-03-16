using System.Collections;
using UnityEngine;

public class WindEvent : MonoBehaviour
{
	public WindZone windZone;

	public float eventLength = 1f;

	public float strengthModifier = 1f;

	public AnimationCurve windStrength;

	public bool isBattlefieldWind;

	public bool isPlaying;

	public ParticleSystem particles;

	[Range(0f, 1f)]
	public float amountToTriggerBattlefieldParticles = 1f;

	private float playTime;

	private bool wasPlaying;

	private void Start()
	{
		if (isBattlefieldWind)
		{
			BattlefieldManager.battlefieldWind = this;
		}
		Play();
	}

	public void Play()
	{
		wasPlaying = true;
		isPlaying = true;
		if (null != particles)
		{
			particles.Play();
		}
		StartCoroutine(BreakWind());
	}

	private IEnumerator BreakWind()
	{
		while (playTime < eventLength)
		{
			playTime += Time.deltaTime;
			float num = windStrength.Evaluate(playTime / eventLength);
			windZone.windMain = strengthModifier * num;
			if (num > 0.1f)
			{
				BattlefieldManager.EmitWindParticles(num * amountToTriggerBattlefieldParticles);
				BattlefieldManager.TriggerSounds();
				if (null != BattlefieldManager.BattlefieldCritterManager)
				{
					BattlefieldManager.BattlefieldCritterManager.AllSkitter();
				}
			}
			yield return null;
		}
		WindEvent windEvent = this;
		WindEvent windEvent2 = this;
		bool flag = false;
		windEvent2.wasPlaying = false;
		windEvent.isPlaying = flag;
		playTime = 0f;
		yield return null;
	}

	private void OnDestroy()
	{
		BattlefieldManager.battlefieldWind = null;
	}
}
