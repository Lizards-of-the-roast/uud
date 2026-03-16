using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Wizards.Mtga.Sparky;

[TrackColor(1f, 0f, 0f)]
[TrackClipType(typeof(SparkyChatBoxClip))]
public class SparkyChatBoxTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<SparkyChatBoxMixerBehaviour>.Create(graph, inputCount);
	}
}
