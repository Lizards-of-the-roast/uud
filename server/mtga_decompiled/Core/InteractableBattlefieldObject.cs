using System.Collections;
using System.Collections.Generic;
using Core.Shared.Code.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableBattlefieldObject : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public ParticleSystem clickParticles;

	public float _squishMultiplier;

	public int _squishVibrato = 1;

	public string audioEvent;

	public float particleDelay;

	[Space(10f)]
	[Header("Random click 3-6")]
	public Vector2 offsetUVOnClick = Vector2.zero;

	public ParticleSystem randomClickParticles;

	private Renderer rendererComponent;

	private Animation animationComponent;

	private int randomClickCounter = 3;

	private List<AnimationState> animations = new List<AnimationState>();

	private int progressiveAnimationCounter;

	private bool particleDelayOn;

	private void Start()
	{
		rendererComponent = base.gameObject.GetComponent<Renderer>();
		animationComponent = base.gameObject.GetComponent<Animation>();
		if (!(null != animationComponent))
		{
			return;
		}
		foreach (AnimationState item in animationComponent)
		{
			animations.Add(item);
		}
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		if (particleDelayOn)
		{
			return;
		}
		if (particleDelay != 0f)
		{
			particleDelayOn = true;
			StartCoroutine(ParticleDelay(particleDelay));
		}
		randomClickCounter--;
		if (randomClickCounter <= 0)
		{
			randomClickCounter = Random.Range(3, 6);
			bool flag = false;
			if (offsetUVOnClick != Vector2.zero)
			{
				Vector4 vector = rendererComponent.material.GetVector(ShaderPropertyIds.TextureScaleOffsetPropId);
				if (vector.z >= 10f)
				{
					vector.z = 0f;
				}
				if (vector.w >= 10f)
				{
					vector.w = 0f;
				}
				rendererComponent.material.SetVector(ShaderPropertyIds.TextureScaleOffsetPropId, new Vector4(vector.x, vector.y, vector.z + offsetUVOnClick.x, vector.w + offsetUVOnClick.y));
				flag = true;
			}
			if (null != randomClickParticles)
			{
				randomClickParticles.Play();
				flag = true;
			}
			if (null != animationComponent && animationComponent.GetClipCount() > 0)
			{
				animationComponent.Play(animations[progressiveAnimationCounter].name);
				if (progressiveAnimationCounter < animations.Count - 1)
				{
					progressiveAnimationCounter++;
				}
				flag = true;
			}
			if (flag)
			{
				return;
			}
		}
		if (!string.IsNullOrEmpty(audioEvent))
		{
			AudioManager.PlayAudio(audioEvent, base.gameObject);
		}
		if (null != clickParticles)
		{
			clickParticles.Play();
		}
		if (_squishMultiplier != 0f && base.transform.localScale.y < 1.2f && base.transform.localScale.y > 0.8f)
		{
			DOTween.Punch(() => base.transform.localScale, delegate(Vector3 x)
			{
				base.transform.localScale = x;
			}, Vector3.one * _squishMultiplier, 1f, _squishVibrato, 10f).SetOptions(AxisConstraint.Y);
		}
	}

	private IEnumerator ParticleDelay(float secs)
	{
		yield return new WaitForSeconds(secs);
		particleDelayOn = false;
	}

	private void Update()
	{
		if (base.transform.localScale.y > 1.1f || base.transform.localScale.y < 0.9f)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.one, Time.deltaTime);
		}
	}
}
