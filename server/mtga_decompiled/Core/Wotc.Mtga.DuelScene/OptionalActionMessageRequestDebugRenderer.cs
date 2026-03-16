using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class OptionalActionMessageRequestDebugRenderer : BaseUserRequestDebugRenderer<OptionalActionMessageRequest>
{
	public OptionalActionMessageRequestDebugRenderer(OptionalActionMessageRequest optionalActionMessageRequest)
		: base(optionalActionMessageRequest)
	{
	}

	public override void Render()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("YES"))
		{
			_request.SubmitResponse(OptionResponse.AllowYes);
		}
		if (GUILayout.Button("NO"))
		{
			_request.SubmitResponse(OptionResponse.CancelNo);
		}
		GUILayout.EndHorizontal();
	}
}
