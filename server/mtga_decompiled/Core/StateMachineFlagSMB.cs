using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga;

public class StateMachineFlagSMB : SMBehaviour
{
	private const string DEFAULT_USER = "default";

	[Tooltip("StateMachine parameter name")]
	public string FlagName;

	[Tooltip("Save flag locally for restoration at startup")]
	public bool PersistentOnDevice;

	protected override void OnEnter()
	{
		Animator.SetBool(FlagName, value: true);
		if (PersistentOnDevice)
		{
			SetPersistentFlag();
		}
	}

	public bool HasPersistentFlag()
	{
		if (Animator == null)
		{
			return false;
		}
		string item = Animator.name + ":" + FlagName;
		return new List<string>(MDNPlayerPrefs.GetStateMachineFlags(GetUserId()).Split(',')).Contains(item);
	}

	public void SetPersistentFlag()
	{
		if (!(Animator == null))
		{
			string item = Animator.name + ":" + FlagName;
			string userId = GetUserId();
			List<string> list = new List<string>(MDNPlayerPrefs.GetStateMachineFlags(userId).Split(','));
			if (!list.Contains(item))
			{
				list.Add(item);
			}
			MDNPlayerPrefs.SetStateMachineFlags(userId, string.Join(",", list));
		}
	}

	public void ClearPersistentFlag(bool resetFlag = false)
	{
		if (!(Animator == null))
		{
			if (resetFlag)
			{
				Animator.SetBool(FlagName, value: false);
			}
			string item = Animator.name + ":" + FlagName;
			string userId = GetUserId();
			List<string> list = new List<string>(MDNPlayerPrefs.GetStateMachineFlags(userId).Split(','));
			list.Remove(item);
			MDNPlayerPrefs.SetStateMachineFlags(userId, string.Join(",", list));
		}
	}

	public static void RestorePersistentFlags(Animator stateMachine)
	{
		if (stateMachine == null)
		{
			return;
		}
		string text = stateMachine.name + ":";
		foreach (string item in new List<string>(MDNPlayerPrefs.GetStateMachineFlags(GetUserId()).Split(',')))
		{
			if (item.StartsWith(text))
			{
				string text2 = item.Substring(text.Length);
				stateMachine.SetBool(text2, value: true);
			}
		}
	}

	public static void ClearPersistentFlags()
	{
		MDNPlayerPrefs.ClearStateMachineFlags(GetUserId());
	}

	private static string GetUserId()
	{
		string text = Pantry.Get<IAccountClient>()?.AccountInformation?.PersonaID;
		if (text == null)
		{
			text = "default";
			Debug.LogError("Failed to get UserId for NPE state machine flags. Some or all state machine flags will be stored/retrieved via the default user, specific only to this device.");
		}
		return text;
	}
}
