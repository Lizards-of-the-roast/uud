namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public sealed class AutoPlayAction_Log : AutoPlayAction
{
	private string _message;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_message = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnExecute()
	{
		Complete(_message);
	}
}
