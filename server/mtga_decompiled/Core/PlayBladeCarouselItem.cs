using System;
using Assets.Core.Shared.Code;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Carousel/PlayBlade", fileName = "PlayBladeCarouselItem")]
public class PlayBladeCarouselItem : CarouselItemBase
{
	[Space(20f)]
	public string EventPublicName;

	protected override bool OnIsVisibleToPlayer()
	{
		EventContext eventContext = WrapperController.Instance.EventManager.EventContexts.Find((EventContext c) => c.PlayerEvent.EventUXInfo.PublicEventName == EventPublicName);
		if (eventContext == null || eventContext.PlayerEvent == null)
		{
			return false;
		}
		if (eventContext.PlayerEvent.EventInfo.EventState == MDNEventState.ForceActive)
		{
			return true;
		}
		if (eventContext.PlayerEvent.EventInfo.EventState == MDNEventState.NotActive)
		{
			return false;
		}
		DateTime gameTime = ServerGameTime.GameTime;
		if (!eventContext.PlayerEvent.EventInfo.IsArenaPlayModeEvent && !MDNPlayerPrefs.AllPlayModesToggle)
		{
			return false;
		}
		if (eventContext.PlayerEvent.EventInfo.StartTime > gameTime)
		{
			return false;
		}
		if (eventContext.PlayerEvent.EventInfo.LockedTime < gameTime)
		{
			return false;
		}
		return true;
	}

	public override bool IsTimeToShow()
	{
		EventContext eventContext = WrapperController.Instance.EventManager.EventContexts.Find((EventContext c) => c.PlayerEvent.EventUXInfo.PublicEventName == EventPublicName);
		if (eventContext != null && eventContext.PlayerEvent.EventInfo.EventState == MDNEventState.ForceActive)
		{
			return true;
		}
		return base.IsTimeToShow();
	}

	public override void OnClick()
	{
		SceneLoader.GetSceneLoader().ShowPlayBladeAndSelect(EventPublicName);
	}
}
