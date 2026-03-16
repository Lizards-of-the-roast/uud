using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class RemoveDuplicatesFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly IFaceInfoGenerator _nestedGenerator;

	private readonly IEqualityComparer<ICardDataAdapter> _cardEqualityComparer;

	private readonly List<FaceHanger.FaceCardInfo> _results = new List<FaceHanger.FaceCardInfo>();

	public RemoveDuplicatesFaceInfoGenerator(IFaceInfoGenerator nestedGenerator, IEqualityComparer<ICardDataAdapter> cardEqualityComparer)
	{
		_nestedGenerator = nestedGenerator ?? new NullFaceInfoGenerator();
		_cardEqualityComparer = cardEqualityComparer;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_results.Clear();
		foreach (FaceHanger.FaceCardInfo item in _nestedGenerator.GenerateFaceCardInfo(cardData, sourceModel))
		{
			if (!IsDuplicateHanger(item.CardData))
			{
				_results.Add(item);
			}
		}
		return _results;
	}

	private bool IsDuplicateHanger(ICardDataAdapter cardData)
	{
		foreach (FaceHanger.FaceCardInfo result in _results)
		{
			if (_cardEqualityComparer.Equals(cardData, result.CardData))
			{
				return true;
			}
		}
		return false;
	}
}
