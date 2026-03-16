using Unity.VisualScripting;
using Wotc.Mtga.Events;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitCategory("NPE")]
public class UserSkippedColorChallenge : Unit
{
	[DoNotSerialize]
	[PortLabel("User Skipped Color Challenge")]
	private ValueOutput _userSkippedColorChallengeOutput;

	[DoNotSerialize]
	[PortLabel("Override Status")]
	private ValueInput _overrideStatus;

	private bool _userSkippedColorChallenge;

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
		_overrideStatus = ValueInput("_overrideStatus", UnitStatusOverride.NoOverride);
		_userSkippedColorChallengeOutput = ValueOutput("_userSkippedColorChallengeOutput", (Flow x) => _userSkippedColorChallenge);
		Succession(_enter, _exit);
	}

	private ControlOutput Enter(Flow flow)
	{
		_userSkippedColorChallenge = Pantry.Get<IColorChallengeStrategy>().ColorChallengeSkipped;
		return _exit;
	}
}
