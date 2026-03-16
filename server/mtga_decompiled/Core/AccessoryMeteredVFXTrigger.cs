using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.VFX;

public class AccessoryMeteredVFXTrigger : MonoBehaviour
{
	[Header("Clicks will be registered during the following trigger states")]
	private Animator anim;

	private bool trigger;

	[SerializeField]
	private ParticleSystem triggerFX;

	[SerializeField]
	private VFXPrefabPlayer triggerPrefabPlayer;

	[SerializeField]
	private List<string> triggerAnimStates;

	[SerializeField]
	private List<string> invokeAnimStates;

	private void Start()
	{
		trigger = false;
		anim = GetComponentInChildren<Animator>();
	}

	public void PlayVFXOnClick()
	{
		if (trigger)
		{
			return;
		}
		foreach (string triggerAnimState in triggerAnimStates)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).IsName(triggerAnimState))
			{
				trigger = true;
				if (triggerFX != null)
				{
					triggerFX.Play();
				}
				else if (triggerPrefabPlayer != null)
				{
					triggerPrefabPlayer.Play();
				}
				break;
			}
		}
	}

	private void Update()
	{
		if (!trigger)
		{
			return;
		}
		anim.GetCurrentAnimatorStateInfo(0);
		anim.GetCurrentAnimatorClipInfo(0);
		foreach (string invokeAnimState in invokeAnimStates)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).IsName(invokeAnimState) && float.Parse((1f - anim.GetCurrentAnimatorStateInfo(0).normalizedTime).ToString("F2")) == 0f)
			{
				trigger = false;
			}
		}
	}
}
