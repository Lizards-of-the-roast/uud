using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public interface ICardView
{
	uint InstanceId { get; }

	ICardDataAdapter Model { get; }

	void SetModel(ICardDataAdapter model);
}
