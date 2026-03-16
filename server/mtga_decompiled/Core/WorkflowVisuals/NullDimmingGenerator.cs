namespace WorkflowVisuals;

public sealed class NullDimmingGenerator : IDimmingGenerator
{
	public Dimming GetDimming()
	{
		return new Dimming();
	}
}
