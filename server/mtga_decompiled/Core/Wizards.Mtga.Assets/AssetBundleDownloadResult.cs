using System;
using System.Collections.Generic;

namespace Wizards.Mtga.Assets;

public class AssetBundleDownloadResult
{
	public int TotalCompleted;

	public bool IsFailure;

	public List<AssetException> Exceptions = new List<AssetException>(4);

	public double LatencyMin = double.MaxValue;

	public double LatencyMax = double.MinValue;

	public double LatencyTotal;

	public double LatencyAvg => LatencyTotal / (double)TotalCompleted;

	public void Add(AssetBundleDownloadResult otherResult)
	{
		TotalCompleted += otherResult.TotalCompleted;
		IsFailure |= otherResult.IsFailure;
		Exceptions.AddRange(otherResult.Exceptions);
		LatencyMin = Math.Min(LatencyMin, otherResult.LatencyMin);
		LatencyMax = Math.Max(LatencyMax, otherResult.LatencyMax);
		LatencyTotal += otherResult.LatencyTotal;
	}

	public void RecordLatency(double ms)
	{
		LatencyMin = Math.Min(LatencyMin, ms);
		LatencyMax = Math.Max(LatencyMax, ms);
		LatencyTotal += ms;
	}
}
