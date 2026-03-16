namespace WorkflowVisuals;

public sealed class NullArrowsGenerator : IArrowsGenerator
{
	public Arrows GetArrows()
	{
		return Arrows.GetDefault();
	}
}
