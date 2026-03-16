using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class NullFaceInfoGenerator : IFaceInfoGenerator
{
	public static readonly IFaceInfoGenerator Default = new NullFaceInfoGenerator();

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		return (IReadOnlyCollection<FaceHanger.FaceCardInfo>)(object)Array.Empty<FaceHanger.FaceCardInfo>();
	}
}
