using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CtoDoneDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOption_DoneRequest>
{
	public CtoDoneDebugRenderer(CastingTimeOption_DoneRequest request)
		: base(request)
	{
	}

	public override void Render()
	{
		if (GUILayout.Button("Normal Cast"))
		{
			_request.SubmitDone();
		}
	}
}
