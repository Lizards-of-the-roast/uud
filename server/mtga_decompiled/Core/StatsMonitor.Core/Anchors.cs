namespace StatsMonitor.Core;

public class Anchors
{
	public Anchor upperLeft = new Anchor(0f, 0f, 0f, 1f, 0f, 1f, 0f, 1f);

	public Anchor upperCenter = new Anchor(0f, 0f, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f);

	public Anchor upperRight = new Anchor(0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f);

	public Anchor lowerRight = new Anchor(0f, 0f, 1f, 0f, 1f, 0f, 1f, 0f);

	public Anchor lowerCenter = new Anchor(0f, 0f, 0.5f, 0f, 0.5f, 0f, 0.5f, 0f);

	public Anchor lowerLeft = new Anchor(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
}
