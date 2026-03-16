namespace WorkflowVisuals;

public sealed class NullHighlightsGenerator : IHighlightsGenerator
{
	public static readonly IHighlightsGenerator Default = new NullHighlightsGenerator();

	public Highlights GetHighlights()
	{
		return new Highlights();
	}
}
