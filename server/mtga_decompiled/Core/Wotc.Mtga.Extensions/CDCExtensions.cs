using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Extensions;

public static class CDCExtensions
{
	public static bool IsParentOf(this DuelScene_CDC thisCard, DuelScene_CDC otherCard)
	{
		if (thisCard == null || otherCard == null || otherCard.Model == null || otherCard.Model.Parent == null)
		{
			return false;
		}
		return thisCard.InstanceId == otherCard.Model.Parent.InstanceId;
	}

	public static bool IsChildOf(this DuelScene_CDC thisCard, DuelScene_CDC otherCard)
	{
		if (thisCard == null || otherCard == null)
		{
			return false;
		}
		return thisCard.IsChildOf(otherCard.InstanceId);
	}

	public static bool IsChildOf(this DuelScene_CDC thisCard, uint otherCardId)
	{
		if (thisCard == null)
		{
			return false;
		}
		if (thisCard.Model.Parent != null)
		{
			return thisCard.Model.Parent.InstanceId == otherCardId;
		}
		return false;
	}

	public static Visibility Visibility(this DuelScene_CDC thisCard)
	{
		if (thisCard == null || thisCard.Model == null)
		{
			return Wotc.Mtgo.Gre.External.Messaging.Visibility.None;
		}
		return thisCard.Model.Visibility;
	}
}
