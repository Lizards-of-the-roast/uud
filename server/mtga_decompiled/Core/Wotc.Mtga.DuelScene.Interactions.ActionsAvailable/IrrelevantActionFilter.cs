using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class IrrelevantActionFilter : IListFilter<GreInteraction>
{
	public void Filter(ref List<GreInteraction> list)
	{
		int num = 0;
		while (num < list.Count)
		{
			if (IsIrrelevant(list[num]))
			{
				list.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private bool IsIrrelevant(GreInteraction interaction)
	{
		return interaction.Type switch
		{
			ActionType.None => true, 
			ActionType.Pass => true, 
			ActionType.ActivateTest => true, 
			_ => false, 
		};
	}
}
