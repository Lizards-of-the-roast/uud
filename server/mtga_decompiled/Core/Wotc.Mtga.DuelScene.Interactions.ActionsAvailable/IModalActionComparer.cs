using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IModalActionComparer : IComparer<Action>
{
	void SetCompareParams(ICardDataAdapter cardModel);

	void ClearCompareParams();
}
