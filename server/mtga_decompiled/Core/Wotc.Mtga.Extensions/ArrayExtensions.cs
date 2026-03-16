using System;
using System.Collections.Generic;

namespace Wotc.Mtga.Extensions;

public static class ArrayExtensions
{
	public static IEnumerable<T> AsEnumerable<T>(this Array array)
	{
		foreach (object item in array)
		{
			if (item is T)
			{
				yield return (T)item;
			}
		}
	}
}
