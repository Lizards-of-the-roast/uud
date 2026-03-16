using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.DuelScene;

namespace Wotc.Mtga.Hangers;

public class HighlightedFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly IFaceInfoGenerator _wrappedFaceInfoGenerator;

	private readonly IHighlightProvider _highlightProvider;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public HighlightedFaceInfoGenerator(IFaceInfoGenerator baseFaceInfoGenerator, IHighlightProvider highlightProvider)
	{
		_wrappedFaceInfoGenerator = baseFaceInfoGenerator ?? new NullFaceInfoGenerator();
		_highlightProvider = highlightProvider ?? new NullHighlightProvider();
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		foreach (FaceHanger.FaceCardInfo item in _wrappedFaceInfoGenerator.GenerateFaceCardInfo(cardData, sourceModel))
		{
			_data.Add(new FaceHanger.FaceCardInfo(item.CardData, item.HeaderText, item.ArrowData, item.HangerType, _highlightProvider));
		}
		return _data;
	}
}
