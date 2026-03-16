using System.Collections.Generic;
using GreClient.CardData;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class FromIntoInfoFaceGenerator : IFaceInfoGenerator
{
	public struct FromIntoArrows
	{
		public FaceHanger.FaceHangerArrowType FromArrow;

		public FaceHanger.FaceHangerArrowType IntoArrow;

		public static FromIntoArrows Default => new FromIntoArrows
		{
			FromArrow = FaceHanger.FaceHangerArrowType.None,
			IntoArrow = FaceHanger.FaceHangerArrowType.Directional
		};
	}

	private readonly IFaceInfoGenerator _fromInfoGenerator;

	private readonly IFaceInfoGenerator _intoInfoGenerator;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	protected FromIntoInfoFaceGenerator(LinkedFace fromLinkedFace, LinkedFace intoLinkedFace, FaceHanger.HangerType hangerType, string fromLocKey, string intoLocKey, FromIntoArrows arrows, LinkedFaceInfoGenerator.Options options, IAbilityDataProvider abilityProvider, IClientLocProvider locManager, IObjectPool genericPool, IComparer<CardPrintingData> cardOrderComparer = null)
	{
		_fromInfoGenerator = new LinkedFaceInfoGenerator(fromLinkedFace, intoLinkedFace, fromLocKey, hangerType, options.WithArrow(arrows.FromArrow), abilityProvider, locManager, genericPool, cardOrderComparer);
		_intoInfoGenerator = new LinkedFaceInfoGenerator(intoLinkedFace, fromLinkedFace, intoLocKey, hangerType, options.WithArrow(arrows.IntoArrow), abilityProvider, locManager, genericPool, cardOrderComparer);
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		AddRangeToData(_fromInfoGenerator, _data, cardData, sourceModel);
		AddRangeToData(_intoInfoGenerator, _data, cardData, sourceModel);
		return _data;
	}

	private static void AddRangeToData(IFaceInfoGenerator faceGenerator, HashSet<FaceHanger.FaceCardInfo> data, ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		foreach (FaceHanger.FaceCardInfo item in faceGenerator.GenerateFaceCardInfo(cardData, sourceModel))
		{
			data.Add(item);
		}
	}
}
