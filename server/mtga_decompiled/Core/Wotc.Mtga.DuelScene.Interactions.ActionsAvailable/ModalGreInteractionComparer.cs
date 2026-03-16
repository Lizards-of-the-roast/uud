using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ModalGreInteractionComparer : IModalGreInteractionComparer, IComparer<GreInteraction>
{
	private readonly IModalActionComparer _actionComparer = ActionComparerFactory.CreateModalActionComparer();

	public void ClearCompareParams()
	{
		_actionComparer.ClearCompareParams();
	}

	public int Compare(GreInteraction x, GreInteraction y)
	{
		return _actionComparer.Compare(x.GreAction, y.GreAction);
	}

	public void SetCompareParams(ICardDataAdapter cardModel)
	{
		_actionComparer.SetCompareParams(cardModel);
	}
}
