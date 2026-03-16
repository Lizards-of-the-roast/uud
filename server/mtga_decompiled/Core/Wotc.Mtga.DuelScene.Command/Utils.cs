using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Command;

public static class Utils
{
	public static bool CanAddCard(MtgCardInstance toAdd, IEnumerable<MtgCardInstance> existingInstances)
	{
		if (toAdd == null)
		{
			return true;
		}
		if (existingInstances == null)
		{
			return true;
		}
		if (toAdd.ObjectType == GameObjectType.Emblem || toAdd.ObjectType == GameObjectType.Boon)
		{
			return true;
		}
		foreach (MtgCardInstance existingInstance in existingInstances)
		{
			if (InstancesMatchGrpIdAndParentId(existingInstance, toAdd))
			{
				return false;
			}
		}
		return true;
	}

	private static bool InstancesMatchGrpIdAndParentId(MtgCardInstance instance1, MtgCardInstance instance2)
	{
		if (instance1 == null || instance2 == null)
		{
			return false;
		}
		if (instance1.GrpId == instance2.GrpId)
		{
			return instance1.ParentId == instance2.ParentId;
		}
		return false;
	}
}
