using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ActionFilterAggregate : IListFilter<GreInteraction>
{
	private readonly IListFilter<GreInteraction>[] _elements;

	public ActionFilterAggregate(params IListFilter<GreInteraction>[] elements)
	{
		_elements = elements ?? Array.Empty<IListFilter<GreInteraction>>();
	}

	public void Filter(ref List<GreInteraction> list)
	{
		IListFilter<GreInteraction>[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].Filter(ref list);
		}
	}
}
