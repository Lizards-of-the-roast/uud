using Unity.VisualScripting;
using Wizards.GeneralUtilities;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitTitle("Object Visibility Event")]
[UnitCategory("NPE")]
public class ObjectVisibilityEvent : EventUnit<GameEventDetails>
{
	public const string VisibilityEventRaisedEventName = "ObjectEventVisibilityRaised";

	[DoNotSerialize]
	private ValueOutput _visibilityEventRaised;

	protected override bool register => true;

	public override EventHook GetHook(GraphReference reference)
	{
		return new EventHook("ObjectEventVisibilityRaised");
	}

	protected override void Definition()
	{
		base.Definition();
		_visibilityEventRaised = ValueOutput<GameEventDetails>("_visibilityEventRaised");
	}

	protected override void AssignArguments(Flow flow, GameEventDetails visibilityEventRaised)
	{
		flow.SetValue(_visibilityEventRaised, visibilityEventRaised);
	}
}
