namespace WorkflowVisuals;

public sealed class NullButtonsGenerator : IButtonsGenerator
{
	public Buttons GetButtons()
	{
		return new Buttons();
	}
}
