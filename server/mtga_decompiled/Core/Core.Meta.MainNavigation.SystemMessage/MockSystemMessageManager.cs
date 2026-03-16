using System;
using System.Collections.Generic;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation.SystemMessage;

public class MockSystemMessageManager : ISystemMessageManager
{
	public void SetFDConnectionWrapper(IFrontDoorConnectionServiceWrapper fdc)
	{
	}

	public SystemMessageManager.SystemMessageHandle ShowOkCancel(string title, string text, Action onOk, Action onCancel, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other, string logOverride = null)
	{
		return null;
	}

	public SystemMessageManager.SystemMessageHandle ShowOk(string title, string text, Action onOk = null, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other, string logOverride = null)
	{
		return null;
	}

	public SystemMessageManager.SystemMessageHandle ShowMessage(string title, string text, List<SystemMessageManager.SystemMessageButtonData> buttons, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other, string logOverride = null)
	{
		return null;
	}

	public SystemMessageManager.SystemMessageHandle ShowMessage(string title, string text, string button1Text, Action button1Action, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other, string logOverride = null)
	{
		return null;
	}

	public SystemMessageManager.SystemMessageHandle ShowMessage(string title, string text, string button1Text, Action button1Action, string button2Text, Action button2Action, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other, string logOverride = null)
	{
		return null;
	}

	public SystemMessageManager.SystemMessageHandle ShowMessage(string title, string text, string button1Text, Action button1Action, string button2Text, Action button2Action, string button3Text, Action button3Action, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other)
	{
		return null;
	}

	public SystemMessageManager.SystemMessageHandle ShowMessage(string title, string text, string button1Text, string button1AlertText, bool button1Disabled, Action button1Action, string button2Text, string button2AlertText, bool button2Disabled, Action button2Action, string button3Text, string button3AlertText, bool button3Disabled, Action button3Action, string details = null, SystemMessageManager.SystemMessagePriority priority = SystemMessageManager.SystemMessagePriority.Other)
	{
		return null;
	}

	public void Close(SystemMessageManager.SystemMessageHandle msg)
	{
	}

	public void ClearMessageQueue()
	{
	}
}
