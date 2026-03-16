using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public abstract class TextComponentController : IComponentController
{
	protected TextComponent _component;

	protected abstract LocalizedTextData GetTextData(IPlayerEvent playerEvent);

	internal TextComponentController(TextComponent component)
	{
		_component = component;
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		bool active = false;
		LocalizedTextData textData = GetTextData(playerEvent);
		if (!string.IsNullOrWhiteSpace(textData?.LocKey))
		{
			active = true;
			_component.SetText(textData.LocKey);
		}
		_component.gameObject.UpdateActive(active);
	}
}
