using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DestructableBattlefieldObject : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public GameObject initialState;

	public GameObject finalDestructionState;

	public ParticleSystem clickVFX;

	public bool cleanUpOnComplete = true;

	private int clickCount;

	private bool blockAnimationProgression;

	private Animation anim;

	private List<AnimationState> animations = new List<AnimationState>();

	public void Start()
	{
		if (!(initialState != null))
		{
			return;
		}
		anim = initialState.GetComponent<Animation>();
		if (!(null != anim))
		{
			return;
		}
		foreach (AnimationState item in anim)
		{
			animations.Add(item);
		}
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		clickVFX.Play();
		if (blockAnimationProgression)
		{
			return;
		}
		switch (clickCount)
		{
		case 4:
			blockAnimationProgression = true;
			anim.Play(animations[0].name);
			StartCoroutine(RestoreClickabilityAfterSecs(3f));
			break;
		case 10:
			blockAnimationProgression = true;
			anim.Play(animations[1].name);
			StartCoroutine(RestoreClickabilityAfterSecs(3f));
			break;
		case 15:
			initialState.SetActive(value: false);
			finalDestructionState.SetActive(value: true);
			if (cleanUpOnComplete)
			{
				StartCoroutine(CleanupAfterDestruction(2f));
			}
			base.gameObject.GetComponent<Collider>().enabled = false;
			break;
		}
		clickCount++;
	}

	private IEnumerator RestoreClickabilityAfterSecs(float secs)
	{
		yield return new WaitForSeconds(secs);
		blockAnimationProgression = false;
	}

	private IEnumerator CleanupAfterDestruction(float secs)
	{
		yield return new WaitForSeconds(secs);
		Object.Destroy(finalDestructionState);
	}
}
