internal class TryActivateAction : ScriptedAction
{
	public uint CardToActivate { get; private set; }

	public TryActivateAction(uint cardForAction)
	{
		CardToActivate = cardForAction;
	}
}
