using System.Collections;
using AssetLookupTree;
using Core.Code.Input;
using EventPage.Components;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

namespace EventPage.CampaignGraph;

public abstract class EventTemplate : MonoBehaviour
{
	protected CampaignGraphContentController _eventPageController;

	public EventPageStates EventPageState { get; private set; }

	public abstract EventContext EventContext { get; }

	public CampaignGraphContentController EventPage
	{
		get
		{
			if (_eventPageController == null)
			{
				_eventPageController = GetComponentInParent<CampaignGraphContentController>();
			}
			return _eventPageController;
		}
	}

	public EventTrackModule EventTrackModule { get; protected set; }

	public virtual void DisableMainButton()
	{
	}

	public abstract void Init(AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actionSystem, CosmeticsProvider cosmetics, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder);

	public abstract void Show();

	public abstract void UpdateTemplate();

	public abstract void Hide();

	public void SetProgressBarState(EventPageStates progressBarState)
	{
		if (progressBarState != EventPageState)
		{
			EventPageState = progressBarState;
			UpdateModules();
			SetProgressBarStateInternal(progressBarState);
		}
	}

	protected abstract void SetProgressBarStateInternal(EventPageStates progressBarState);

	protected abstract void ShowModules();

	public abstract void UpdateModules();

	public abstract IEnumerator PlayAnimation(EventTemplateAnimation animation);

	protected abstract void OnEventRewardsPanelClosed();
}
