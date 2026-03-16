using System;
using System.Globalization;
using UnityEngine.Profiling;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_MemoryReport : AutoPlayAction
{
	private string _marker;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_marker = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnExecute()
	{
		GC.Collect();
		long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
		long totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong();
		long monoUsedSizeLong = Profiler.GetMonoUsedSizeLong();
		long monoHeapSizeLong = Profiler.GetMonoHeapSizeLong();
		long allocatedMemoryForGraphicsDriver = Profiler.GetAllocatedMemoryForGraphicsDriver();
		string text = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
		Complete($"Memory (u/U/m/M/g)|{text}|{_marker}|{totalAllocatedMemoryLong}|{totalReservedMemoryLong}|{monoUsedSizeLong}|{monoHeapSizeLong}|{allocatedMemoryForGraphicsDriver}|");
	}
}
