using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class SubmitDeckRequestDebugRenderer : BaseUserRequestDebugRenderer<SubmitDeckRequest>
{
	public SubmitDeckRequestDebugRenderer(SubmitDeckRequest request)
		: base(request)
	{
	}

	public override void Render()
	{
		if (GUILayout.Button("Submit"))
		{
			_request.SubmitDeck(_request.Deck);
		}
	}
}
