using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.CardView;

public class NullVisualCardProvider : IVisualStateCardProvider
{
	public static readonly IVisualStateCardProvider Default = new NullVisualCardProvider();

	public IEnumerable<DuelScene_CDC> GetCardViews()
	{
		return Array.Empty<DuelScene_CDC>();
	}
}
