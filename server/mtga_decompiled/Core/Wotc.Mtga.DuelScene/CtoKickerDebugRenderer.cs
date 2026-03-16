using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CtoKickerDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOption_KickerRequest>
{
	public CtoKickerDebugRenderer(CastingTimeOption_KickerRequest request)
		: base(request)
	{
	}

	public override void Render()
	{
		if (GUILayout.Button("Kicker"))
		{
			_request.SubmitKicked();
		}
	}
}
