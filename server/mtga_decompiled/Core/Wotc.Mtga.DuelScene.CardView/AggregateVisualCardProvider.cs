using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.CardView;

public class AggregateVisualCardProvider : IVisualStateCardProvider
{
	private readonly IVisualStateCardProvider[] _elements;

	public AggregateVisualCardProvider(params IVisualStateCardProvider[] elements)
	{
		_elements = elements ?? Array.Empty<IVisualStateCardProvider>();
	}

	public IEnumerable<DuelScene_CDC> GetCardViews()
	{
		IVisualStateCardProvider[] elements = _elements;
		foreach (IVisualStateCardProvider visualStateCardProvider in elements)
		{
			foreach (DuelScene_CDC cardView in visualStateCardProvider.GetCardViews())
			{
				yield return cardView;
			}
		}
	}
}
