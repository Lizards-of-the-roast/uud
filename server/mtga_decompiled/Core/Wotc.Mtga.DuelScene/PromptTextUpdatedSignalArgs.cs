namespace Wotc.Mtga.DuelScene;

public class PromptTextUpdatedSignalArgs : SignalArgs
{
	public readonly string PromptText;

	public PromptTextUpdatedSignalArgs(object dispatcher, string promptText)
		: base(dispatcher)
	{
		PromptText = promptText;
	}
}
