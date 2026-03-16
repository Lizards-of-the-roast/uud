using UnityEngine;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph;

public class EventDescriptionModule : EventModule
{
	[SerializeField]
	private Localize _descriptionLocalize;

	[SerializeField]
	private EventTextType _textType = EventTextType.Description;

	public override void Show()
	{
		UpdateModule();
	}

	public override void UpdateModule()
	{
		if (string.IsNullOrWhiteSpace(base.EventContext.PlayerEvent?.GetLocalizedText(_textType)))
		{
			base.gameObject.UpdateActive(active: false);
			return;
		}
		base.gameObject.UpdateActive(active: true);
		_descriptionLocalize.SetText(base.EventContext.PlayerEvent?.GetLocalizedText(_textType));
	}

	public override void Hide()
	{
		base.gameObject.UpdateActive(active: false);
	}
}
