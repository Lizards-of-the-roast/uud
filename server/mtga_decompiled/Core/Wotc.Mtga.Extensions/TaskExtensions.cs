using System;
using System.Threading.Tasks;

namespace Wotc.Mtga.Extensions;

public static class TaskExtensions
{
	public static async Task WithTimeout(this Task task, TimeSpan timeout)
	{
		if (task == await Task.WhenAny(task, Task.Delay(timeout)))
		{
			await task;
			return;
		}
		throw new TimeoutException();
	}

	public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
	{
		if (task == await Task.WhenAny(task, Task.Delay(timeout)))
		{
			return await task;
		}
		throw new TimeoutException();
	}
}
