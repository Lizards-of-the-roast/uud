using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class CountLimitedFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly uint _maxCount;

	private readonly IFaceInfoGenerator _nestedGenerator;

	private readonly List<FaceHanger.FaceCardInfo> _results = new List<FaceHanger.FaceCardInfo>();

	public CountLimitedFaceInfoGenerator(uint maxCount, IFaceInfoGenerator nestedGenerator)
	{
		_maxCount = maxCount;
		_nestedGenerator = nestedGenerator ?? new NullFaceInfoGenerator();
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_results.Clear();
		foreach (FaceHanger.FaceCardInfo item in _nestedGenerator.GenerateFaceCardInfo(cardData, sourceModel))
		{
			if (_results.Count >= _maxCount)
			{
				break;
			}
			_results.Add(item);
		}
		return _results;
	}
}
