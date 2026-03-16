using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.CardView;

public class CDCPart_CastableStank : CDCPart
{
	protected override void HandleEnableRenderersInternal(bool enabled)
	{
		base.gameObject.UpdateActive(enabled);
	}
}
