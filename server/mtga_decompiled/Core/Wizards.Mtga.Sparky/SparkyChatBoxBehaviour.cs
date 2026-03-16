using UnityEngine;
using UnityEngine.Playables;

namespace Wizards.Mtga.Sparky;

public class SparkyChatBoxBehaviour : PlayableBehaviour
{
	private SparkyController _sparkyController;

	private string _textIdentifier;

	private Vector3 _chatBoxOffset;

	public SparkyController SparkyController
	{
		get
		{
			return _sparkyController;
		}
		set
		{
			_sparkyController = value;
		}
	}

	public string TextIdentifier
	{
		get
		{
			return _textIdentifier;
		}
		set
		{
			_textIdentifier = value;
		}
	}

	public Vector3 ChatBoxOffset
	{
		get
		{
			return _chatBoxOffset;
		}
		set
		{
			_chatBoxOffset = value;
		}
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		if (_sparkyController == null)
		{
			Debug.LogError("We don't have the sparky controller. Can't open the chat box!");
		}
		else
		{
			_sparkyController.Say(TextIdentifier, _chatBoxOffset, (float)playable.GetDuration());
		}
	}
}
