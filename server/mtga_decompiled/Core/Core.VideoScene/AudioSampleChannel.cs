using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core.VideoScene;

public class AudioSampleChannel
{
	private readonly Queue<AudioSample> _samplesQueue = new Queue<AudioSample>(10000);

	[CanBeNull]
	private AudioSample _partialSample;

	public uint BufferSize { get; set; } = 1024u;

	public bool EnablePartialBuffer { get; set; } = true;

	public void WriteBuffer(int startIdx, int endIdx, float[] buffer)
	{
		int num = startIdx;
		while (num < endIdx)
		{
			AudioSample audioSample = (EnablePartialBuffer ? (_partialSample ?? new AudioSample
			{
				Data = new float[BufferSize]
			}) : new AudioSample
			{
				Data = new float[BufferSize]
			});
			long num2 = Math.Min(BufferSize - audioSample.DataIdx, endIdx - num);
			Array.Copy(buffer, num, audioSample.Data, audioSample.DataIdx, num2);
			num += (int)num2;
			if (EnablePartialBuffer && num2 < BufferSize)
			{
				audioSample.DataIdx = (int)num2;
				_partialSample = audioSample;
			}
			else
			{
				_samplesQueue.Enqueue(audioSample);
			}
		}
	}

	public float[] ReadBuffer()
	{
		if (!_samplesQueue.TryDequeue(out var result))
		{
			return new float[BufferSize];
		}
		return result.Data;
	}
}
