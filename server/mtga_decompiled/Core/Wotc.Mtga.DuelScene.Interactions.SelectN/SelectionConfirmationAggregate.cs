using System;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class SelectionConfirmationAggregate : ISelectionConfirmation
{
	private ISelectionConfirmation[] _elements;

	public SelectionConfirmationAggregate(params ISelectionConfirmation[] elements)
	{
		_elements = elements ?? Array.Empty<ISelectionConfirmation>();
	}

	public string GetConfirmationText(HighlightType highlightType, IEntityView entityView, SelectNRequest request)
	{
		ISelectionConfirmation[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			string confirmationText = elements[i].GetConfirmationText(highlightType, entityView, request);
			if (!string.IsNullOrEmpty(confirmationText))
			{
				return confirmationText;
			}
		}
		return null;
	}
}
