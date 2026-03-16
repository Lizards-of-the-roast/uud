using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Wizards.GeneralUtilities.ObjectCommunication;

namespace Wizards.Mtga.Sparky;

[Serializable]
[DisplayName("Sparky Chat Box")]
public class SparkyChatBoxClip : PlayableAsset, ITimelineClipAsset, IPropertyPreview
{
	[SerializeField]
	private BeaconIdentifier _sparkyControllerIdentifier;

	[SerializeField]
	private string _textIdentifier;

	[SerializeField]
	private Vector3 _chatBoxOffset;

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<SparkyChatBoxBehaviour> scriptPlayable = ScriptPlayable<SparkyChatBoxBehaviour>.Create(graph);
		SparkyChatBoxBehaviour behaviour = scriptPlayable.GetBehaviour();
		SparkyController sparkyController = _sparkyControllerIdentifier.GetBeaconObject<SparkyController>()[0];
		if (sparkyController == null)
		{
			Debug.LogErrorFormat("Couldn't find the SparkyController with the supplied identifier.\nIdentifier: {0}", _sparkyControllerIdentifier);
			return scriptPlayable;
		}
		behaviour.SparkyController = sparkyController;
		behaviour.TextIdentifier = _textIdentifier;
		behaviour.ChatBoxOffset = _chatBoxOffset;
		return scriptPlayable;
	}

	public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
	}
}
