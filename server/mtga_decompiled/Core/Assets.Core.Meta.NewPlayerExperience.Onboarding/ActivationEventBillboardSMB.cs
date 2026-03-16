using System;
using UnityEngine;

namespace Assets.Core.Meta.NewPlayerExperience.Onboarding;

public class ActivationEventBillboardSMB : ActivationSMB
{
	[SerializeField]
	private string _eventId;

	protected override void OnEnter()
	{
		GameObject[] sceneObjects = SceneObjectReference.GetSceneObjects(_targetPath);
		foreach (GameObject gameObject in sceneObjects)
		{
			HomePageBillboard componentInParent = gameObject.GetComponentInParent<HomePageBillboard>();
			if (!(componentInParent == null) && componentInParent.EventId.Equals(_eventId, StringComparison.InvariantCulture))
			{
				_target = gameObject;
				break;
			}
		}
		if (_target == null && Application.isEditor)
		{
			Debug.LogWarning("Cannot find activation target at " + _targetPath + " on " + Animator.name, Animator);
		}
		UpdateActive();
		if (_revertOnStateMachineClose)
		{
			StateMachineEvents stateMachineEvents = GetStateMachineEvents();
			stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Combine(stateMachineEvents.OnStateMachineDisable, new Action(base.Revert));
		}
	}
}
