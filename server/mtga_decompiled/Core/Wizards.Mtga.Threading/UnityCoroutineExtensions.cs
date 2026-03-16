using System.Threading.Tasks;
using UnityEngine;

namespace Wizards.Mtga.Threading;

public static class UnityCoroutineExtensions
{
	public static CustomYieldInstruction WaitYield(this Task task, out Task taskResult)
	{
		taskResult = task;
		return new WaitUntil(() => task.IsCompleted);
	}

	public static CustomYieldInstruction WaitYield<T>(this Task<T> task, out Task<T> taskResult)
	{
		taskResult = task;
		return new WaitUntil(() => task.IsCompleted);
	}
}
