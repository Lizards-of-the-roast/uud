namespace Wotc.Mtga.DuelScene.Logging;

public class NullConfirmZeroLogger : IConfirmZeroLogger
{
	public static readonly IConfirmZeroLogger Default = new NullConfirmZeroLogger();

	public void ConfirmZeroDisplayed(uint sourceId)
	{
	}

	public void ConfirmZeroSelected()
	{
	}

	public void BackSelected()
	{
	}

	public void UndoSelected()
	{
	}

	public void WorkflowCleanup()
	{
	}
}
