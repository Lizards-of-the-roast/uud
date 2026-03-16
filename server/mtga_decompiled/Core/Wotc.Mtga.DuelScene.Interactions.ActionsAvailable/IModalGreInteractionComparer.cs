using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IModalGreInteractionComparer : IComparer<GreInteraction>
{
	void SetCompareParams(ICardDataAdapter cardModel);

	void ClearCompareParams();
}
