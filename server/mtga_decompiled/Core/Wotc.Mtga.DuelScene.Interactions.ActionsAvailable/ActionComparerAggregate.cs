using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ActionComparerAggregate : IModalActionComparer, IComparer<Wotc.Mtgo.Gre.External.Messaging.Action>
{
	private readonly IComparer<Wotc.Mtgo.Gre.External.Messaging.Action>[] _elements;

	private readonly HashSet<IModalActionComparer> _modalComparers = new HashSet<IModalActionComparer>();

	public ActionComparerAggregate(params IComparer<Wotc.Mtgo.Gre.External.Messaging.Action>[] elements)
	{
		_elements = elements ?? Array.Empty<IComparer<Wotc.Mtgo.Gre.External.Messaging.Action>>();
		IComparer<Wotc.Mtgo.Gre.External.Messaging.Action>[] elements2 = _elements;
		foreach (IComparer<Wotc.Mtgo.Gre.External.Messaging.Action> comparer in elements2)
		{
			_modalComparers.AddIfNotNull(comparer as IModalActionComparer);
		}
	}

	public void SetCompareParams(ICardDataAdapter cardModel)
	{
		foreach (IModalActionComparer modalComparer in _modalComparers)
		{
			modalComparer.SetCompareParams(cardModel);
		}
	}

	public void ClearCompareParams()
	{
		foreach (IModalActionComparer modalComparer in _modalComparers)
		{
			modalComparer.ClearCompareParams();
		}
	}

	public int Compare(Wotc.Mtgo.Gre.External.Messaging.Action x, Wotc.Mtgo.Gre.External.Messaging.Action y)
	{
		IComparer<Wotc.Mtgo.Gre.External.Messaging.Action>[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			int num = elements[i].Compare(x, y);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}
}
