using Unity.VisualScripting;
using Wotc.Mtga.Events;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitCategory("NPE")]
public class GetColorMasteryCompletedTracksCount : Unit
{
	[DoNotSerialize]
	[PortLabel("Mastery Track Completion Count")]
	private ValueOutput _completedTracksCount;

	[DoNotSerialize]
	[PortLabel("Override")]
	private ValueInput _override;

	[DoNotSerialize]
	[PortLabel("Override Value")]
	private ValueInput _overrideValue;

	private int _masterTrackCountCompletion;

	[PortLabelHidden]
	[DoNotSerialize]
	public ControlInput _enter { get; private set; }

	[PortLabelHidden]
	[DoNotSerialize]
	public ControlOutput _exit { get; private set; }

	protected override void Definition()
	{
		_enter = ControlInput("_enter", Enter);
		_exit = ControlOutput("_exit");
		_override = ValueInput("_override", @default: false);
		_overrideValue = ValueInput("_overrideValue", 0);
		_completedTracksCount = ValueOutput("_completedTracksCount", (Flow x) => _masterTrackCountCompletion);
		Succession(_enter, _exit);
	}

	private ControlOutput Enter(Flow flow)
	{
		_masterTrackCountCompletion = Pantry.Get<IColorChallengeStrategy>().CompletedTracks.Count;
		return _exit;
	}
}
