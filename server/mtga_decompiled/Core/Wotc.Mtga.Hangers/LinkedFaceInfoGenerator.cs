using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class LinkedFaceInfoGenerator : IFaceInfoGenerator
{
	public struct Options
	{
		public FaceHanger.FaceHangerArrowType ArrowDirection;

		public bool HideForCopies;

		public bool CopyPerpetualEffects;

		public static Options Default => new Options
		{
			ArrowDirection = FaceHanger.FaceHangerArrowType.None,
			HideForCopies = false,
			CopyPerpetualEffects = false
		};

		public static Options DefaultHideForCopies => new Options
		{
			ArrowDirection = FaceHanger.FaceHangerArrowType.None,
			HideForCopies = true,
			CopyPerpetualEffects = false
		};

		public Options(Options copy)
		{
			ArrowDirection = copy.ArrowDirection;
			HideForCopies = copy.HideForCopies;
			CopyPerpetualEffects = copy.CopyPerpetualEffects;
		}

		public Options WithArrow(FaceHanger.FaceHangerArrowType arrow)
		{
			return new Options
			{
				ArrowDirection = arrow,
				HideForCopies = HideForCopies,
				CopyPerpetualEffects = CopyPerpetualEffects
			};
		}
	}

	private readonly IAbilityDataProvider _abilityProvider;

	private readonly IClientLocProvider _localizationManager;

	private readonly IComparer<CardPrintingData> _colorComparer;

	private readonly IObjectPool _genericPool;

	private readonly LinkedFace _linkedFaceToShow;

	private readonly LinkedFace _requiredLinkedFace;

	private readonly string _headerLocKey;

	private readonly FaceHanger.HangerType _type;

	private readonly Options _options;

	private readonly FaceHanger.FaceHangerArrowData _arrowData;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public LinkedFaceInfoGenerator(LinkedFace linkedFaceToShow, LinkedFace requiredLinkedFace, string headerLocKey, FaceHanger.HangerType type, Options options, IAbilityDataProvider abilityProvider, IClientLocProvider locManager, IObjectPool genericPool, IComparer<CardPrintingData> colorComparer = null)
	{
		_linkedFaceToShow = linkedFaceToShow;
		_requiredLinkedFace = requiredLinkedFace;
		_headerLocKey = headerLocKey;
		_type = type;
		_arrowData = new FaceHanger.FaceHangerArrowData(options.ArrowDirection);
		_options = options;
		_abilityProvider = abilityProvider ?? NullAbilityDataProvider.Default;
		_localizationManager = locManager ?? NullLocProvider.Default;
		_genericPool = genericPool;
		_colorComparer = colorComparer;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (_options.HideForCopies && cardData.Instance != null && (cardData.Instance.IsCopy || cardData.Instance.IsObjectCopy))
		{
			return _data;
		}
		if (cardData?.LinkedFaceType != _requiredLinkedFace)
		{
			return _data;
		}
		List<CardPrintingData> list = _genericPool.PopObject<List<CardPrintingData>>();
		list.AddRange(LinkedFacePrintings(cardData, _linkedFaceToShow));
		if (_colorComparer != null)
		{
			list.Sort(_colorComparer);
		}
		foreach (CardPrintingData item2 in list)
		{
			MtgCardInstance mtgCardInstance = item2.CreateInstance();
			mtgCardInstance.SkinCode = sourceModel.SkinCode;
			ICardDataAdapter cardDataAdapter = new CardData(mtgCardInstance, item2);
			if (_options.CopyPerpetualEffects && sourceModel.HasPerpetualChanges())
			{
				PerpetualChangeUtilities.CopyPerpetualEffects(sourceModel, cardDataAdapter, _abilityProvider);
			}
			FaceHanger.FaceCardInfo item = new FaceHanger.FaceCardInfo(cardDataAdapter, _localizationManager.GetLocalizedText(_headerLocKey), _arrowData, _type);
			_data.Add(item);
		}
		list.Clear();
		_genericPool.PushObject(list, tryClear: false);
		return _data;
	}

	private static IEnumerable<CardPrintingData> LinkedFacePrintings(ICardDataAdapter cardData, LinkedFace linkedFace)
	{
		if (cardData == null || cardData.LinkedFacePrintings == null)
		{
			yield break;
		}
		foreach (CardPrintingData linkedFacePrinting in cardData.LinkedFacePrintings)
		{
			if (linkedFacePrinting.LinkedFaceType == linkedFace)
			{
				yield return linkedFacePrinting;
			}
		}
	}
}
