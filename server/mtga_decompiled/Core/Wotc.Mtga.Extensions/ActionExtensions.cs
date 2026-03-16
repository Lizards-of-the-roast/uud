using System;

namespace Wotc.Mtga.Extensions;

public static class ActionExtensions
{
	public static void SafeInvoke(this Action a)
	{
		if (a == null)
		{
			return;
		}
		Delegate[] invocationList = a.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action action = (Action)invocationList[i];
			try
			{
				action();
			}
			catch (Exception e)
			{
				SimpleLog.LogException(e);
			}
		}
	}

	public static void SafeInvoke<T>(this Action<T> a, T value)
	{
		if (a == null)
		{
			return;
		}
		Delegate[] invocationList = a.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action<T> action = (Action<T>)invocationList[i];
			try
			{
				action(value);
			}
			catch (Exception e)
			{
				SimpleLog.LogException(e);
			}
		}
	}

	public static void SafeInvoke<T1, T2>(this Action<T1, T2> a, T1 value1, T2 value2)
	{
		if (a == null)
		{
			return;
		}
		Delegate[] invocationList = a.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action<T1, T2> action = (Action<T1, T2>)invocationList[i];
			try
			{
				action(value1, value2);
			}
			catch (Exception e)
			{
				SimpleLog.LogException(e);
			}
		}
	}
}
