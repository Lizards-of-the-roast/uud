using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class SpecializesIntoGenerator : IFaceInfoGenerator
{
	private class SpecializePrintingComparer : IComparer<CardPrintingData>
	{
		private CardColor _baseColor;

		private readonly IComparer<CardColor> _colorComparer = new WUBRGColorComparer();

		public void SetBaseColor(CardColor color)
		{
			_baseColor = color;
		}

		public int Compare(CardPrintingData x, CardPrintingData y)
		{
			CardColor specializeColor = GetSpecializeColor(_baseColor, x);
			CardColor specializeColor2 = GetSpecializeColor(_baseColor, y);
			if (specializeColor == _baseColor)
			{
				return -1;
			}
			return _colorComparer.Compare(specializeColor, specializeColor2);
		}
	}

	private static FaceHanger.FaceHangerArrowData _arrowData = new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.Directional);

	private readonly IClientLocProvider _clientLocProvider;

	private readonly IObjectPool _genericPool;

	private readonly List<FaceHanger.FaceCardInfo> _cache = new List<FaceHanger.FaceCardInfo>();

	private readonly SpecializePrintingComparer _specializePrintingsComparer = new SpecializePrintingComparer();

	public SpecializesIntoGenerator(IClientLocProvider clientLocProvider, IObjectPool genericPool)
	{
		_clientLocProvider = clientLocProvider ?? new NullLocProvider();
		_genericPool = genericPool;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_cache.Clear();
		_specializePrintingsComparer.SetBaseColor(GetBaseColor(cardData));
		List<CardPrintingData> list = _genericPool.PopObject<List<CardPrintingData>>();
		list.AddRange(SpecializePrintings(cardData));
		list.Sort(_specializePrintingsComparer);
		foreach (CardPrintingData item2 in list)
		{
			MtgCardInstance mtgCardInstance = item2.CreateInstance();
			mtgCardInstance.SkinCode = cardData.SkinCode;
			FaceHanger.FaceCardInfo item = new FaceHanger.FaceCardInfo(new CardData(mtgCardInstance, item2), _clientLocProvider.GetLocalizedText("DuelScene/FaceHanger/SpecializesInto"), _arrowData, FaceHanger.HangerType.Specialized);
			_cache.Add(item);
		}
		list.Clear();
		_genericPool.PushObject(list, tryClear: false);
		return _cache;
	}

	private IEnumerable<CardPrintingData> SpecializePrintings(ICardDataAdapter cardData)
	{
		if (cardData == null || cardData.LinkedFacePrintings == null)
		{
			yield break;
		}
		foreach (CardPrintingData linkedFacePrinting in cardData.LinkedFacePrintings)
		{
			if (linkedFacePrinting.LinkedFaceType == LinkedFace.SpecializeParent)
			{
				yield return linkedFacePrinting;
			}
		}
	}

	private static CardColor GetBaseColor(ICardDataAdapter card)
	{
		if (card.Colors != null && card.Colors.Count > 0)
		{
			return card.Colors[0];
		}
		return CardColor.Colorless;
	}

	private static CardColor GetSpecializeColor(CardColor baseColor, CardPrintingData printing)
	{
		IReadOnlyList<CardColor> colors = printing.Colors;
		if (colors != null && colors.Count == 1)
		{
			return printing.Colors[0];
		}
		IReadOnlyList<CardColor> colors2 = printing.Colors;
		if (colors2 != null && colors2.Count == 0)
		{
			return CardColor.Colorless;
		}
		foreach (CardColor color in printing.Colors)
		{
			if (color != baseColor)
			{
				return color;
			}
		}
		return printing.Colors[0];
	}
}
