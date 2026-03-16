using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace Wizards.Mtga;

public class MemoryManager : IDisposable
{
	private const float MEMORY_DIVIDER = 1048576f;

	public MemoryManager()
	{
		Application.lowMemory += ApplicationOnLowMemory;
	}

	private void ApplicationOnLowMemory()
	{
		string text = "[Memory] Low memory warning\n";
		text += $"Unity Used = {(float)Profiler.GetTotalAllocatedMemoryLong() / 1048576f}\n";
		text += $"Unity Reserved = {(float)Profiler.GetTotalReservedMemoryLong() / 1048576f}\n";
		text += $"Unity Free = {(float)Profiler.GetTotalUnusedReservedMemoryLong() / 1048576f}\n";
		text += $"Managed Used = {(float)Profiler.GetMonoUsedSizeLong() / 1048576f}\n";
		text += $"Managed Reserved = {(float)Profiler.GetMonoHeapSizeLong() / 1048576f}\n";
		Debug.Log("[Memory] Low memory warning\n" + text);
		PAPA.StartGlobalCoroutine(PAPA.ClearPapaCache());
	}

	public void Dispose()
	{
		Application.lowMemory -= ApplicationOnLowMemory;
	}
}
