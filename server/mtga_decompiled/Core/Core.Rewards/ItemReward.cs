using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Rewards;

[Serializable]
public abstract class ItemReward<T, P> : RewardBase<P> where P : Component
{
	public Queue<T> ToAdd = new Queue<T>();

	private readonly List<string> _uniqueIds = new List<string>();

	public Func<T, string> GetUniqueId;

	public override int ToAddCount => ToAdd.Count;

	public void AddItemIfUnique(T item)
	{
		if (GetUniqueId != null)
		{
			string item2 = GetUniqueId(item);
			if (!_uniqueIds.Contains(item2))
			{
				_uniqueIds.Add(item2);
				ToAdd.Enqueue(item);
			}
		}
		else
		{
			Debug.LogError("[Rewards] Calling AddItemIfUnique without GetUniqueId being set");
		}
	}

	public override void ClearAdded()
	{
		ToAdd.Clear();
		_uniqueIds.Clear();
	}
}
