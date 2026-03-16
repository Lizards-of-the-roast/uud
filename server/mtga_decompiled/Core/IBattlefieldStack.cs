using System.Collections.Generic;
using GreClient.CardData;

public interface IBattlefieldStack
{
	bool HasAttachmentOrExile { get; }

	List<DuelScene_CDC> AllCards { get; }

	DuelScene_CDC StackParent { get; }

	ICardDataAdapter StackParentModel { get; }

	List<DuelScene_CDC> StackedCards { get; }

	uint Age { get; }

	DuelScene_CDC OldestCard { get; }

	DuelScene_CDC YoungestCard { get; }

	bool IsAttackStack { get; }

	bool IsBlockStack { get; }

	int AttachmentCount { get; }

	int ExileCount { get; }

	void RefreshAbilitiesBasedOnStackPosition();
}
