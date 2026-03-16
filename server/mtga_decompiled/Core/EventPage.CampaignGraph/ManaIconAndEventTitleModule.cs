using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph;

public class ManaIconAndEventTitleModule : EventModule
{
	[SerializeField]
	private Localize _eventTitleLocalize;

	[SerializeField]
	private Image _manaIcon;

	private AssetLoader.AssetTracker<Sprite> _manaIconSpriteTracker;

	public override void Show()
	{
		base.gameObject.SetActive(value: true);
		_eventTitleLocalize.SetText(base.EventContext.PlayerEvent?.GetLocalizedText(EventTextType.Title_EventPage));
		if (_manaIcon != null)
		{
			if (_manaIconSpriteTracker == null)
			{
				_manaIconSpriteTracker = new AssetLoader.AssetTracker<Sprite>("EventTitleManaIconSprite");
			}
			AssetLoaderUtils.TrySetSprite(path: (!(_parentTemplate is CampaignGraphEventTemplate campaignGraphEventTemplate)) ? ClientEventDefinitionList.GetManaIconPath_Title(_assetLookupSystem, base.EventContext) : ClientEventDefinitionList.GetManaIconPath_Title(_assetLookupSystem, campaignGraphEventTemplate.CurrentTrackName), image: _manaIcon, assetTracker: _manaIconSpriteTracker);
		}
	}

	public override void UpdateModule()
	{
	}

	public override void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_manaIcon, _manaIconSpriteTracker);
	}
}
