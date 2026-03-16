using Unity.VisualScripting;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitTitle("Page Changed Event")]
[UnitCategory("NPE")]
public class PageChangeEvent : EventUnit<NavContentType>
{
	public const string PageChangedEventName = "PageChangedEvent";

	[DoNotSerialize]
	private ValueOutput _changedTo;

	protected override bool register => true;

	public override EventHook GetHook(GraphReference reference)
	{
		return new EventHook("PageChangedEvent");
	}

	protected override void Definition()
	{
		base.Definition();
		_changedTo = ValueOutput<NavContentType>("_changedTo");
	}

	protected override void AssignArguments(Flow flow, NavContentType navigationWindowChangedTo)
	{
		flow.SetValue(_changedTo, navigationWindowChangedTo);
	}
}
