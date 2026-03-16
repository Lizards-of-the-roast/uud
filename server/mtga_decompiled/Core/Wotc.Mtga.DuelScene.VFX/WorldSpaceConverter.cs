using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class WorldSpaceConverter : ISpaceConverter
{
	private readonly Transform _worldSpaceTransform;

	public WorldSpaceConverter(Transform worldSpaceTransform)
	{
		_worldSpaceTransform = worldSpaceTransform;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		set.Add(_worldSpaceTransform);
	}
}
