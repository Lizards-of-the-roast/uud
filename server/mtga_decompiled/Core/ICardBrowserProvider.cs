using AssetLookupTree.Blackboard;

public interface ICardBrowserProvider : IDuelSceneBrowserProvider
{
	bool ApplyTargetOffset { get; }

	bool ApplySourceOffset { get; }

	bool ApplyControllerOffset { get; }

	string GetCardHolderLayoutKey();

	BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView);

	void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb);
}
