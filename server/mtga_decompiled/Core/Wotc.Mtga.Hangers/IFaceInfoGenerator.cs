using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public interface IFaceInfoGenerator
{
	IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel);
}
