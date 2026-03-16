using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioEmitterBehavior : MonoBehaviour
{
	public enum ActionType
	{
		Play,
		Stop,
		Pause,
		Resume,
		Break,
		Release
	}

	public enum SpaceType
	{
		Local,
		Global
	}

	[Serializable]
	public struct AudioAction
	{
		public ActionType Action;

		public SpaceType Space;

		public string EventName;

		public float Delay;
	}

	[Header("[Actions fired when this object is shown]")]
	[SerializeField]
	private AudioAction[] _enableActions = new AudioAction[0];

	[Header("[Actions fired when this object is hidden]")]
	[SerializeField]
	private AudioAction[] _disableActions = new AudioAction[0];

	private void OnEnable()
	{
		PerformActions(_enableActions);
	}

	private void OnDisable()
	{
		if ((bool)base.gameObject)
		{
			PerformActions(_disableActions);
		}
	}

	private void PerformActions(IEnumerable<AudioAction> audioActions)
	{
		foreach (AudioAction audioAction in audioActions)
		{
			GameObject obj = AudioManager.Default;
			if (audioAction.Space == SpaceType.Local)
			{
				obj = base.gameObject;
			}
			if (audioAction.Action == ActionType.Play)
			{
				AudioManager.PlayAudio(audioAction.EventName, obj, audioAction.Delay);
			}
			else
			{
				AudioManager.ExecuteActionOnEvent(audioAction.EventName, (AkActionOnEventType)(audioAction.Action - 1), obj);
			}
		}
	}
}
