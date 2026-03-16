using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class MulliganRequestDebugRenderer : BaseUserRequestDebugRenderer<MulliganRequest>
{
	public MulliganRequestDebugRenderer(MulliganRequest mulliganRequest)
		: base(mulliganRequest)
	{
	}

	public override void Render()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Mulligan"))
		{
			_request.MulliganHand();
		}
		if (GUILayout.Button("Keep"))
		{
			_request.KeepHand();
		}
		GUILayout.EndHorizontal();
	}
}
