using System;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGo.Social.Models;
using UnityEngine;

namespace HasbroGo;

public class ChangeSocialPresencePanel : MonoBehaviour
{
	[SerializeField]
	private GameObject busyPresenceButtonGO;

	private readonly string logCategory = "ChangeSocialPresencePanel";

	public void ShowBusyPresenceButton(bool show)
	{
		busyPresenceButtonGO.SetActive(show);
	}

	public void SetPresenceButtonClicked(string newPlatformStatusString)
	{
		if (Enum.TryParse<PlatformStatus>(newPlatformStatusString, out var result))
		{
			SocialManager.Instance.UpdatePresence(result);
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Invalid PlatformStatus string provided: " + newPlatformStatusString);
		}
	}
}
