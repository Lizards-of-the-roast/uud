using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SpinTrigger : MonoBehaviour
{
	[Serializable]
	private class VeloctiyEvent
	{
		public enum Acceleration
		{
			accelerating,
			decelerating
		}

		public float velocityThreshold = 10f;

		public Acceleration triggerWhile;

		[HideInInspector]
		public bool isTriggered;

		public UnityEvent velocityReachedEvent;

		public void Invoke()
		{
			velocityReachedEvent.Invoke();
		}
	}

	[HideInInspector]
	public float velocity;

	[SerializeField]
	private float velocityAddOnClick = 10f;

	[SerializeField]
	private float velocityMin = 10f;

	[SerializeField]
	private float velocityMax = 200f;

	[SerializeField]
	private float portalOpenVelocity = 50f;

	[SerializeField]
	private float portalOpenTime = 5f;

	[SerializeField]
	private float drag = 0.1f;

	[SerializeField]
	private float speedMod = 10f;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string triggerOnMax;

	[SerializeField]
	private string dontTriggerDuringState;

	[SerializeField]
	private UnityEvent VelocityThresholdReachedEvents;

	[SerializeField]
	private UnityEvent PortalIsOpenClickEvents;

	[SerializeField]
	private VeloctiyEvent[] VelocityTriggeredEvents;

	[SerializeField]
	private UnityEvent[] TriggerOnSpinnableClick;

	private float portalOpenTimeCounter;

	private bool isSpinnable;

	private IEnumerator VelocityTriggers()
	{
		while (true)
		{
			VeloctiyEvent[] velocityTriggeredEvents = VelocityTriggeredEvents;
			foreach (VeloctiyEvent veloctiyEvent in velocityTriggeredEvents)
			{
				if (veloctiyEvent.triggerWhile == VeloctiyEvent.Acceleration.accelerating)
				{
					if (velocity > veloctiyEvent.velocityThreshold && !veloctiyEvent.isTriggered)
					{
						veloctiyEvent.Invoke();
						veloctiyEvent.isTriggered = true;
					}
					if (velocity < veloctiyEvent.velocityThreshold && veloctiyEvent.isTriggered)
					{
						veloctiyEvent.isTriggered = false;
					}
				}
				if (veloctiyEvent.triggerWhile == VeloctiyEvent.Acceleration.decelerating)
				{
					if (velocity < veloctiyEvent.velocityThreshold && !veloctiyEvent.isTriggered)
					{
						veloctiyEvent.Invoke();
						veloctiyEvent.isTriggered = true;
					}
					if (velocity > veloctiyEvent.velocityThreshold && veloctiyEvent.isTriggered)
					{
						veloctiyEvent.isTriggered = false;
					}
				}
			}
			yield return null;
		}
	}

	private void Start()
	{
		StartCoroutine(VelocityTriggers());
		VeloctiyEvent[] velocityTriggeredEvents = VelocityTriggeredEvents;
		foreach (VeloctiyEvent veloctiyEvent in velocityTriggeredEvents)
		{
			if (veloctiyEvent.triggerWhile == VeloctiyEvent.Acceleration.decelerating)
			{
				veloctiyEvent.isTriggered = true;
			}
		}
	}

	private void Update()
	{
		if (animator.GetBool("isPortalOpen"))
		{
			portalOpenTimeCounter += Time.deltaTime;
			if (portalOpenTimeCounter >= portalOpenTime)
			{
				animator.SetBool("isPortalOpen", value: false);
				portalOpenTimeCounter = 0f;
			}
			return;
		}
		if (velocity > 0f)
		{
			base.transform.Rotate(0f, speedMod * velocity * Time.deltaTime, 0f, Space.Self);
		}
		if (velocity < 0f)
		{
			velocity = 0f;
		}
		if (velocity == 0f)
		{
			base.transform.localEulerAngles = Vector3.zero;
		}
		if (velocity > velocityMin)
		{
			velocity = Mathf.MoveTowards(velocity, 0f, drag * Time.deltaTime);
		}
		if (velocity < velocityMin && base.transform.localEulerAngles.y % 180f < 2f)
		{
			velocity = 0f;
			base.transform.localEulerAngles = Vector3.zero;
		}
		if (velocity > velocityMax && base.transform.localEulerAngles.y % 180f < 2f)
		{
			VelocityThresholdReachedEvents.Invoke();
			velocity = 0f;
			base.transform.localEulerAngles = Vector3.zero;
			animator.SetTrigger(triggerOnMax);
			velocity = portalOpenVelocity;
		}
		if (animator.GetCurrentAnimatorStateInfo(0).IsName(dontTriggerDuringState))
		{
			velocity = 0f;
		}
	}

	public void EnableSpinTrigger()
	{
		isSpinnable = true;
	}

	public void SpinMe()
	{
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName(dontTriggerDuringState) && isSpinnable && velocity < velocityMax)
		{
			velocity += velocityAddOnClick;
			UnityEvent[] triggerOnSpinnableClick = TriggerOnSpinnableClick;
			for (int i = 0; i < triggerOnSpinnableClick.Length; i++)
			{
				triggerOnSpinnableClick[i].Invoke();
			}
		}
		if (animator.GetBool("isPortalOpen"))
		{
			PortalIsOpenClickEvents.Invoke();
		}
	}
}
