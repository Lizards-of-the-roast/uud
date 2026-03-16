using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class ParentFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly IFaceInfoGenerator[] _children;

	private HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public ParentFaceInfoGenerator(params IFaceInfoGenerator[] children)
	{
		_children = children ?? new IFaceInfoGenerator[0];
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		IFaceInfoGenerator[] children = _children;
		for (int i = 0; i < children.Length; i++)
		{
			foreach (FaceHanger.FaceCardInfo item in children[i].GenerateFaceCardInfo(cardData, sourceModel))
			{
				_data.Add(item);
			}
		}
		return _data;
	}
}
