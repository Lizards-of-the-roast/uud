using System;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftBoosterViewMock : IDraftBoosterView
{
	public CollationMapping CollationId { get; set; }

	public bool PassDirectionIsLeft { get; set; }

	public bool IsActive { get; set; }

	public IDraftBoosterView PassBooster(bool passDirecitonLeft, Action<IDraftBoosterView> onFinishedAnimation = null)
	{
		PassDirectionIsLeft = passDirecitonLeft;
		DraftBoosterViewMock obj = new DraftBoosterViewMock();
		onFinishedAnimation?.Invoke(obj);
		return this;
	}

	public IDraftBoosterView SetBoosterData(CollationMapping collationId)
	{
		CollationId = collationId;
		return this;
	}

	public IDraftBoosterView UpdateActive(bool isActive)
	{
		IsActive = isActive;
		return this;
	}

	public override bool Equals(object obj)
	{
		return GetHashCode() == obj.GetHashCode();
	}

	public override int GetHashCode()
	{
		return ((-1743435564 * -1521134295 + CollationId.GetHashCode()) * -1521134295 + PassDirectionIsLeft.GetHashCode()) * -1521134295 + IsActive.GetHashCode();
	}
}
