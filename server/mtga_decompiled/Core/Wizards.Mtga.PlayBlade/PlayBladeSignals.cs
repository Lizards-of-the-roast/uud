namespace Wizards.Mtga.PlayBlade;

public readonly struct PlayBladeSignals
{
	public readonly EnterMatchMakingSignal JoinMatchSignal;

	public readonly EditDeckSignal EditDeckSignal;

	public readonly GoToEventPageSignal GoToEventPageSignal;

	public readonly QueueSelectedSignal QueueSelectedSignal;

	public readonly SelectTabSignal SelectTabSignal;

	public readonly ClosePlayBladeSignal ClosePlayBladeSignal;

	public readonly FilterSelectedSignal FilterSelectedSignal;

	public PlayBladeSignals(EnterMatchMakingSignal matchSignal, EditDeckSignal editDeckSignal, GoToEventPageSignal goToEventPageSignal, QueueSelectedSignal queueSelectedSignal, SelectTabSignal selectTabSignal, ClosePlayBladeSignal closePlayBladeSignal, FilterSelectedSignal filterSelectedSignal)
	{
		JoinMatchSignal = matchSignal;
		EditDeckSignal = editDeckSignal;
		GoToEventPageSignal = goToEventPageSignal;
		QueueSelectedSignal = queueSelectedSignal;
		SelectTabSignal = selectTabSignal;
		ClosePlayBladeSignal = closePlayBladeSignal;
		FilterSelectedSignal = filterSelectedSignal;
	}
}
