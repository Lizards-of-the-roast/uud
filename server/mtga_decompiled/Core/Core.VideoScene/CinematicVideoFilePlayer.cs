using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Shared.Code;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Video;
using Wizards.Mtga;

namespace Core.VideoScene;

[RequireComponent(typeof(VideoPlayer))]
public class CinematicVideoFilePlayer : MonoBehaviour
{
	private VideoPlayer _videoPlayer;

	private AudioSampleProvider _audioSampleProvider;

	private bool _isPlaying;

	private uint _audioChannels;

	private uint _audioSampleRate;

	private AssetLookupSystem _assetLookupTree;

	private readonly List<AudioSampleChannel> _audioSampleChannelBuffer = new List<AudioSampleChannel>();

	private const string WwiseEvent = "video_streaming_play";

	private const int WwiseBufferSize = 1024;

	public static string VideoToPlay { get; set; } = "";

	public static string VideoPlayLookupMode { get; set; } = "";

	public static string VideoPlayAudioMode { get; set; } = "";

	private void Awake()
	{
		_assetLookupTree = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_videoPlayer = GetComponent<VideoPlayer>();
	}

	private void Start()
	{
		if (string.IsNullOrWhiteSpace(VideoToPlay))
		{
			Debug.LogError("No Video Set: " + VideoToPlay);
			ReturnToPreviousScene(null, "");
			return;
		}
		_videoPlayer.source = ((VideoPlayLookupMode == "url") ? VideoSource.Url : VideoSource.VideoClip);
		switch (_videoPlayer.source)
		{
		default:
			return;
		case VideoSource.VideoClip:
		{
			_assetLookupTree.Blackboard.Clear();
			_assetLookupTree.Blackboard.LookupString = VideoToPlay;
			VideoClip objectData = AssetLoader.GetObjectData<VideoClip>(_assetLookupTree.TreeLoader.LoadTree<VideoClipPayload>().GetPayload(_assetLookupTree.Blackboard)?.VideoClip?.RelativePath);
			if (objectData == null)
			{
				Debug.LogError("No/invalid Video Set: " + VideoToPlay);
				ReturnToPreviousScene(null, "");
				return;
			}
			_videoPlayer.clip = objectData;
			break;
		}
		case VideoSource.Url:
			_videoPlayer.url = VideoToPlay;
			break;
		}
		_videoPlayer.audioOutputMode = ((VideoPlayAudioMode == "wwise") ? VideoAudioOutputMode.APIOnly : VideoAudioOutputMode.Direct);
		_videoPlayer.prepareCompleted += PlayWhenReady;
		_videoPlayer.errorReceived += ReturnToPreviousScene;
		_videoPlayer.loopPointReached += EndVideo;
		_videoPlayer.Prepare();
	}

	private void PlayWhenReady(VideoPlayer vp)
	{
		_isPlaying = true;
		vp.EnableAudioTrack(0, enabled: true);
		if (vp.audioOutputMode == VideoAudioOutputMode.APIOnly)
		{
			UseWwiseAudio();
		}
		else if (vp.audioOutputMode == VideoAudioOutputMode.Direct)
		{
			UseDirectAudio();
		}
		vp.Play();
	}

	private void UseDirectAudio()
	{
		if (_videoPlayer.canSetDirectAudioVolume)
		{
			float volume = MathF.Min(MDNPlayerPrefs.PLAYERPREFS_KEY_MASTERVOLUME / 100f, 0.8f);
			_videoPlayer.SetDirectAudioVolume(0, volume);
		}
	}

	private void UseWwiseAudio()
	{
		_audioChannels = _videoPlayer.GetAudioChannelCount(0);
		_audioSampleRate = _videoPlayer.GetAudioSampleRate(0);
		_audioSampleProvider = _videoPlayer.GetAudioSampleProvider(0);
		_audioSampleProvider.sampleFramesAvailable += OnAudioFramesAvailable;
		_audioSampleProvider.sampleFramesOverflow += OnAudioFramesOverflow;
		_audioSampleProvider.enableSampleFramesAvailableEvents = true;
		_audioSampleProvider.freeSampleFrameCountLowThreshold = _audioSampleProvider.maxSampleFrameCount / 2;
		for (int i = 0; i < _audioChannels; i++)
		{
			_audioSampleChannelBuffer.Add(new AudioSampleChannel());
		}
		AkAudioInputManager.PostAudioInputEvent("video_streaming_play", base.gameObject, SampleDelegate, FormatDelegate);
	}

	private void OnAudioFramesOverflow(AudioSampleProvider provider, uint sampleFrameCount)
	{
		Debug.LogError("dropping audio frames : " + sampleFrameCount);
	}

	private void OnAudioFramesAvailable(AudioSampleProvider provider, uint sampleFrameCount)
	{
		if (_audioSampleChannelBuffer.Count == 0)
		{
			return;
		}
		using NativeArray<float> sampleFrames = new NativeArray<float>((int)(sampleFrameCount * provider.channelCount), Allocator.Temp);
		uint num = provider.ConsumeSampleFrames(sampleFrames);
		for (int i = 0; i < provider.channelCount; i++)
		{
			long num2 = num * i;
			long num3 = num * (1 + i);
			_audioSampleChannelBuffer[i].WriteBuffer((int)num2, (int)num3, sampleFrames.ToArray());
		}
	}

	private bool SampleDelegate(uint playingId, uint channelIndex, float[] samples)
	{
		if (_audioSampleChannelBuffer.Count == 0)
		{
			return false;
		}
		Array.Copy(_audioSampleChannelBuffer[(int)channelIndex].ReadBuffer(), samples, 1024);
		return _isPlaying;
	}

	private void FormatDelegate(uint playingId, AkAudioFormat format)
	{
		format.channelConfig.uNumChannels = _audioChannels;
		format.uSampleRate = _audioSampleRate;
	}

	private void EndVideo(VideoPlayer vp)
	{
		ReturnToPreviousScene(vp, "");
	}

	private void ReturnToPreviousScene(VideoPlayer vp, string s)
	{
		CleanUp();
		Pantry.Get<GlobalCoroutineExecutor>().StartCoroutine(CinematicScripting.UncoverHomePage(WrapperController.Instance, base.gameObject.scene));
	}

	private void CleanUp()
	{
		_isPlaying = false;
		if (_audioSampleProvider != null)
		{
			_audioSampleProvider.sampleFramesAvailable -= OnAudioFramesAvailable;
			_audioSampleProvider.sampleFramesOverflow -= OnAudioFramesOverflow;
			AkSoundEngine.ExecuteActionOnEvent("video_streaming_play", AkActionOnEventType.AkActionOnEventType_Stop, base.gameObject);
		}
		_audioSampleChannelBuffer.Clear();
		if (_videoPlayer != null)
		{
			_videoPlayer.prepareCompleted -= PlayWhenReady;
			_videoPlayer.errorReceived -= ReturnToPreviousScene;
			_videoPlayer.loopPointReached -= EndVideo;
		}
	}

	private void OnDestroy()
	{
		CleanUp();
	}
}
