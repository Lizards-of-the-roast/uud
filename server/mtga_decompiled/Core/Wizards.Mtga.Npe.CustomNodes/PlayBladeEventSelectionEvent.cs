using Unity.VisualScripting;
using Wizards.Mtga.PlayBlade;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitTitle("Play Blade Selection Event")]
[UnitCategory("NPE")]
public class PlayBladeEventSelectionEvent : EventUnit<BladeEventInfo>
{
	public const string PlayBladeEventSelectionEventName = "PlayBladeSelectionEvent";

	[DoNotSerialize]
	private ValueOutput _changedTo;

	protected override bool register => true;

	public override EventHook GetHook(GraphReference reference)
	{
		return new EventHook("PlayBladeSelectionEvent");
	}

	protected override void Definition()
	{
		base.Definition();
		_changedTo = ValueOutput<BladeEventInfo>("_changedTo");
	}

	protected override void AssignArguments(Flow flow, BladeEventInfo navigationWindowChangedTo)
	{
		flow.SetValue(_changedTo, navigationWindowChangedTo);
	}
}
