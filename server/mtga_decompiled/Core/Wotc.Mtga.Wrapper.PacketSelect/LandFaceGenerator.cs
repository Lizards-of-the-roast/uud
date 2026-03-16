using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class LandFaceGenerator : IFaceInfoGenerator
{
	private IClientLocProvider _locManager;

	private InventoryManager _invManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _output = new HashSet<FaceHanger.FaceCardInfo>();

	public LandFaceGenerator(IClientLocProvider locManager, InventoryManager invManager)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_invManager = invManager;
	}

	~LandFaceGenerator()
	{
		_locManager = null;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_output.Clear();
		string localizedText = _locManager.GetLocalizedText("Events/Packets/Packet_Land_Header_Text");
		FaceHanger.FaceCardInfo.MetaData visualData = default(FaceHanger.FaceCardInfo.MetaData);
		if (_invManager.Cards.TryGetValue(cardData.GrpId, out var value))
		{
			visualData = new FaceHanger.FaceCardInfo.MetaData
			{
				collectionInfo = new FaceHanger.FaceCardInfo.CollectionInfo
				{
					min = (uint)value,
					max = 1u
				}
			};
		}
		_output.Add(new FaceHanger.FaceCardInfo(cardData, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.FrontBack), visualData));
		return _output;
	}
}
