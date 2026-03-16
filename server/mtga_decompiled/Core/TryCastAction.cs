internal class TryCastAction : ScriptedAction
{
	public uint CardToCast { get; private set; }

	public TryCastAction(uint cardForAction)
	{
		CardToCast = cardForAction;
	}
}
