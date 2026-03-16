using System;
using System.Collections.Generic;

namespace Wizards.Mtga.Utils;

public class BinarySemaphore
{
	private List<(object key, Action callback)> accessQueue = new List<(object, Action)>();

	private object activeAccess;

	public void RequestAccess(object key, Action callback)
	{
		if (activeAccess == null)
		{
			activeAccess = key;
			callback?.Invoke();
		}
		else if (activeAccess == key)
		{
			callback?.Invoke();
		}
		else
		{
			accessQueue.Add((key, callback));
		}
	}

	public void RepealAccess(object key)
	{
		for (int num = accessQueue.Count - 1; num >= 0; num--)
		{
			if (accessQueue[num].key == key)
			{
				accessQueue.RemoveAt(num);
			}
		}
		if (activeAccess == key)
		{
			activeAccess = null;
		}
		if (activeAccess == null && accessQueue.Count > 0)
		{
			(object, Action) tuple = accessQueue[0];
			accessQueue.RemoveAt(0);
			(activeAccess, _) = tuple;
			tuple.Item2?.Invoke();
		}
	}
}
