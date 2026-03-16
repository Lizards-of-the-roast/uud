namespace Wotc.Mtga.DuelScene;

public class NullCardHoverController : ICardHoverController
{
	public static NullCardHoverController Default { get; } = new NullCardHoverController();

	public void EndHover()
	{
	}
}
