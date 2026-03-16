using System;
using System.Collections.Generic;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Store;

public class StorePackPool
{
	private Func<StoreItem, StoreItemBase> _spawner;

	private Dictionary<int, List<StoreItemBase>> _pooledPackItems = new Dictionary<int, List<StoreItemBase>>();

	private Dictionary<int, List<StoreItemBase>> _spawnedPackItems = new Dictionary<int, List<StoreItemBase>>();

	public StoreItemBase Spawn(StoreItem storeItem)
	{
		List<StoreItemBase> list = ListForUnitCount(storeItem.PackCount, _pooledPackItems);
		StoreItemBase storeItemBase;
		if (list.Count > 0)
		{
			storeItemBase = list[0];
			list.RemoveAt(0);
		}
		else
		{
			storeItemBase = _spawner(storeItem);
		}
		ListForUnitCount(storeItem.PackCount, _spawnedPackItems).Add(storeItemBase);
		return storeItemBase;
	}

	public void Despawn(int unitCount, StoreItemBase spawnedItem)
	{
		spawnedItem.gameObject.UpdateActive(active: false);
		ListForUnitCount(unitCount, _pooledPackItems).Add(spawnedItem);
		ListForUnitCount(unitCount, _pooledPackItems).Remove(spawnedItem);
	}

	public void DespawnAll()
	{
		foreach (KeyValuePair<int, List<StoreItemBase>> spawnedPackItem in _spawnedPackItems)
		{
			foreach (StoreItemBase item in spawnedPackItem.Value)
			{
				Despawn(spawnedPackItem.Key, item);
			}
			spawnedPackItem.Value.Clear();
		}
	}

	private List<StoreItemBase> ListForUnitCount(int unitCount, Dictionary<int, List<StoreItemBase>> dictionary)
	{
		if (!dictionary.TryGetValue(unitCount, out var value))
		{
			value = (dictionary[unitCount] = new List<StoreItemBase>());
		}
		return value;
	}

	public bool HasSpawnableItemInPool(int unitCount)
	{
		return ListForUnitCount(unitCount, _spawnedPackItems).Count > 0;
	}

	public void Init(Func<StoreItem, StoreItemBase> spawner)
	{
		_spawner = spawner;
	}
}
