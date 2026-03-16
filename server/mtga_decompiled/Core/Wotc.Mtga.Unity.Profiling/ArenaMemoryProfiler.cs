using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Profiling;

namespace Wotc.Mtga.Unity.Profiling;

public class ArenaMemoryProfiler : IDisposable
{
	private const string SYSTEM_USED_MEMORY = "profiler.systemUsedMemory";

	private const string TOTAL_RESERVED_MEMORY = "profiler.totalReservedMemory";

	private const string TOTAL_USED_MEMORY = "profiler.totalUsedMemory";

	private const string GC_RESERVED_MEMORY = "profiler.gcReservedMemory";

	private const string GC_USED_MEMORY = "profiler.gcUsedMemory";

	private const string GFX_RESERVED_MEMORY = "profiler.gfxReservedMemory";

	private const string GFX_USED_MEMORY = "profiler.gfxUsedMemory";

	private const string AUDIO_RESERVED_MEMORY = "profiler.audioReservedMemory";

	private const string AUDIO_USED_MEMORY = "profiler.audioUsedMemory";

	private const string VIDEO_RESERVED_MEMORY = "profiler.videoReservedMemory";

	private const string VIDEO_USED_MEMORY = "profiler.videoUsedMemory";

	private ProfilerRecorder _systemUsedMemory;

	private ProfilerRecorder _totalReservedMemory;

	private ProfilerRecorder _totalUsedMemory;

	private readonly bool _trackGc;

	private ProfilerRecorder _gcReservedMemory;

	private ProfilerRecorder _gcUsedMemory;

	private readonly bool _trackGfx;

	private ProfilerRecorder _gfxReservedMemory;

	private ProfilerRecorder _gfxUsedMemory;

	private readonly bool _trackAudio;

	private ProfilerRecorder _audioReservedMemory;

	private ProfilerRecorder _audioUsedMemory;

	private readonly bool _trackVideo;

	private ProfilerRecorder _videoReservedMemory;

	private ProfilerRecorder _videoUsedMemory;

	public ArenaMemoryProfiler(bool trackGc, bool trackGfx, bool trackAudio, bool trackVideo)
	{
		_systemUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		_totalReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		_totalUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		if (_trackGc = trackGc)
		{
			_gcReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
			_gcUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		}
		if (_trackGfx = trackGfx)
		{
			_gfxReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
			_gfxUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		}
		if (_trackAudio = trackAudio)
		{
			_audioReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
			_audioUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		}
		if (_trackVideo = trackVideo)
		{
			_videoReservedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Video Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
			_videoUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Video Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		}
	}

	public void AddMetricsToDictionary(Dictionary<string, string> dict)
	{
		try
		{
			dict["profiler.systemUsedMemory"] = _systemUsedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			dict["profiler.totalReservedMemory"] = _totalReservedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			dict["profiler.totalUsedMemory"] = _totalUsedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			if (_trackGc)
			{
				dict["profiler.gcReservedMemory"] = _gcReservedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
				dict["profiler.gcUsedMemory"] = _gcUsedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			}
			if (_trackGfx)
			{
				dict["profiler.gfxReservedMemory"] = _gfxReservedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
				dict["profiler.gfxUsedMemory"] = _gfxUsedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			}
			if (_trackAudio)
			{
				dict["profiler.audioReservedMemory"] = _audioReservedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
				dict["profiler.audioUsedMemory"] = _audioUsedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			}
			if (_trackVideo)
			{
				dict["profiler.videoReservedMemory"] = _videoReservedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
				dict["profiler.videoUsedMemory"] = _videoUsedMemory.CurrentValue.ToString(CultureInfo.InvariantCulture);
			}
		}
		catch
		{
		}
	}

	public void Dispose()
	{
		_systemUsedMemory.Dispose();
		_totalReservedMemory.Dispose();
		_totalUsedMemory.Dispose();
		_gcReservedMemory.Dispose();
		_gcUsedMemory.Dispose();
		_gfxReservedMemory.Dispose();
		_gfxUsedMemory.Dispose();
		_audioReservedMemory.Dispose();
		_audioUsedMemory.Dispose();
		_videoReservedMemory.Dispose();
		_videoUsedMemory.Dispose();
	}
}
