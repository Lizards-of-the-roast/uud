namespace Wotc.Mtga.AutoPlay;

public class AutoPlayAction_Fail : AutoPlayAction
{
	private string _failMessage;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_failMessage = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnUpdate()
	{
		Fail(_failMessage);
	}
}
