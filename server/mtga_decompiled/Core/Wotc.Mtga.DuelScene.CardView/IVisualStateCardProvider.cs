using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.CardView;

public interface IVisualStateCardProvider
{
	IEnumerable<DuelScene_CDC> GetCardViews();
}
