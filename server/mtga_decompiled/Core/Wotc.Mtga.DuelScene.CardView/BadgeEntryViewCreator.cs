using Pooling;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

namespace Wotc.Mtga.DuelScene.CardView;

public class BadgeEntryViewCreator
{
	private BadgeEntryView _badgeEntryView;

	private string _badgeEntryViewReference;

	public BadgeEntryViewCreator(BadgeEntryView badgeEntryView)
	{
		_badgeEntryView = badgeEntryView;
	}

	public BadgeEntryViewCreator(string badgeEntryViewReference)
	{
		_badgeEntryViewReference = badgeEntryViewReference;
	}

	public BadgeEntryView Create(IUnityObjectPool objectPool)
	{
		if (_badgeEntryView != null)
		{
			return objectPool.PopObject(_badgeEntryView.gameObject)?.GetComponent<BadgeEntryView>();
		}
		if (!string.IsNullOrWhiteSpace(_badgeEntryViewReference))
		{
			return objectPool.PopObject(_badgeEntryViewReference)?.GetComponent<BadgeEntryView>();
		}
		return null;
	}
}
