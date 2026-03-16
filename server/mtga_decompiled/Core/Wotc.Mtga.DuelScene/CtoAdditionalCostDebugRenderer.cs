using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CtoAdditionalCostDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOption_AdditionalCostRequest>
{
	public CtoAdditionalCostDebugRenderer(CastingTimeOption_AdditionalCostRequest request)
		: base(request)
	{
	}

	public override void Render()
	{
		if (GUILayout.Button("Additional Cost"))
		{
			_request.SubmitAdditionalCost();
		}
	}
}
