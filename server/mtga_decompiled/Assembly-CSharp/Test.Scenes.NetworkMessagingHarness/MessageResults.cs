using System;

namespace Test.Scenes.NetworkMessagingHarness;

public class MessageResults
{
	public string Name;

	public int Iterations = 1;

	public TimeSpan SerTime = TimeSpan.Zero;

	public TimeSpan NetworkTime = TimeSpan.Zero;

	public TimeSpan DecompressTime = TimeSpan.Zero;

	public TimeSpan DeserTime = TimeSpan.Zero;

	public TimeSpan MinPing = TimeSpan.MaxValue;

	public TimeSpan MaxPing = TimeSpan.MinValue;

	public int OutgoingRawMsgSize;

	public int IncomingRawMsgSize;

	public int DecompressedMsgSize;

	public long MinMem = long.MaxValue;

	public long MaxMem = long.MinValue;

	private TimeSpan TotalTime => SerTime + NetworkTime + DeserTime;

	public void AddIteration(MessageResults iteration)
	{
		SerTime = RollingAverage(SerTime, Iterations, iteration.SerTime);
		NetworkTime = RollingAverage(NetworkTime, Iterations, iteration.NetworkTime);
		DecompressTime = RollingAverage(DecompressTime, Iterations, iteration.DecompressTime);
		DeserTime = RollingAverage(DeserTime, Iterations, iteration.DeserTime);
		MinPing = RollingAverage(MinPing, Iterations, iteration.MinPing);
		MaxPing = RollingAverage(MaxPing, Iterations, iteration.MaxPing);
		OutgoingRawMsgSize = RollingAverage(OutgoingRawMsgSize, Iterations, iteration.OutgoingRawMsgSize);
		IncomingRawMsgSize = RollingAverage(IncomingRawMsgSize, Iterations, iteration.IncomingRawMsgSize);
		DecompressedMsgSize = RollingAverage(DecompressedMsgSize, Iterations, iteration.DecompressedMsgSize);
		MinMem = RollingAverage(MinMem, Iterations, iteration.MinMem);
		MaxMem = RollingAverage(MaxMem, Iterations, iteration.MaxMem);
		Iterations++;
	}

	private int RollingAverage(int current, int weight, int next)
	{
		return (current * weight + next) / (weight + 1);
	}

	private long RollingAverage(long current, int weight, long next)
	{
		return (current * weight + next) / (weight + 1);
	}

	private TimeSpan RollingAverage(TimeSpan current, int weight, TimeSpan next)
	{
		return (current * weight + next) / (weight + 1);
	}

	private string DisplayBytes(int bytes)
	{
		if (bytes == int.MaxValue || bytes == int.MinValue)
		{
			return "???";
		}
		if (bytes > 1048576)
		{
			float num = (float)bytes / 1048576f;
			return $"{num:F1} MB";
		}
		if (bytes > 1024)
		{
			float num2 = (float)bytes / 1024f;
			return $"{num2:F1} KB";
		}
		return $"{bytes} bytes";
	}

	private string DisplayBytes(long bytes)
	{
		if (bytes == long.MaxValue || bytes == long.MinValue)
		{
			return "???";
		}
		if (bytes > 1048576)
		{
			float num = (float)bytes / 1048576f;
			return $"{num:F1} MB";
		}
		if (bytes > 1024)
		{
			float num2 = (float)bytes / 1024f;
			return $"{num2:F1} KB";
		}
		return $"{bytes} bytes";
	}

	private string DisplayTimeSpan(TimeSpan t)
	{
		if (t.TotalSeconds <= 1.0)
		{
			return $"{t:s\\.fff} seconds";
		}
		if (t.TotalMinutes <= 1.0)
		{
			return $"{t:s\\.f} seconds";
		}
		return $"{t:%m} minutes";
	}

	public override string ToString()
	{
		return Name + Environment.NewLine + Environment.NewLine + ((Iterations > 1) ? ($"Iterations: {Iterations}" + Environment.NewLine) : "") + "Time: " + DisplayTimeSpan(TotalTime) + Environment.NewLine + "    Serialization: " + DisplayTimeSpan(SerTime) + Environment.NewLine + "    Network: " + DisplayTimeSpan(NetworkTime) + Environment.NewLine + "        Server: Min " + DisplayTimeSpan(NetworkTime - MaxPing) + ", Max " + DisplayTimeSpan(NetworkTime - MinPing) + Environment.NewLine + "        Ping: Min " + DisplayTimeSpan(MinPing) + ", Max " + DisplayTimeSpan(MaxPing) + Environment.NewLine + ((DecompressTime >= TimeSpan.FromMilliseconds(1.0)) ? ("    Decompression: " + DisplayTimeSpan(DecompressTime) + Environment.NewLine) : "") + "    Deserialization: " + DisplayTimeSpan(DeserTime) + Environment.NewLine + "Sizes: Request " + DisplayBytes(OutgoingRawMsgSize) + ", Response " + DisplayBytes(IncomingRawMsgSize) + ((DecompressTime >= TimeSpan.FromMilliseconds(1.0)) ? (", Decompressed " + DisplayBytes(DecompressedMsgSize)) : "") + Environment.NewLine + "Memory: Min " + DisplayBytes(MinMem) + ", Max " + DisplayBytes(MaxMem);
	}
}
