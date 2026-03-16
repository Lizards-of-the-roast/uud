using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.CardView;

public class ViewManagerCardProvider : IVisualStateCardProvider
{
	private readonly EntityViewManager _viewManager;

	public IEnumerable<DuelScene_CDC> GetCardViews()
	{
		if (_viewManager == null)
		{
			return Array.Empty<DuelScene_CDC>();
		}
		return _viewManager.GetAllCards();
	}

	public ViewManagerCardProvider(EntityViewManager viewManager)
	{
		_viewManager = viewManager;
	}
}
