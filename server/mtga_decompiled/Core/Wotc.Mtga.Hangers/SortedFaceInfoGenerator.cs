using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class SortedFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly IComparer<FaceHanger.FaceCardInfo> _comparer;

	private readonly IFaceInfoGenerator _nestedGenerator;

	private readonly List<FaceHanger.FaceCardInfo> _results = new List<FaceHanger.FaceCardInfo>();

	public SortedFaceInfoGenerator(IComparer<FaceHanger.FaceCardInfo> comparer, IFaceInfoGenerator nestedGenerator)
	{
		_comparer = comparer ?? new NullFaceHangerComparer();
		_nestedGenerator = nestedGenerator ?? new NullFaceInfoGenerator();
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_results.Clear();
		foreach (FaceHanger.FaceCardInfo item in _nestedGenerator.GenerateFaceCardInfo(cardData, sourceModel))
		{
			_results.Add(item);
		}
		_results.Sort(_comparer);
		return _results;
	}
}
