using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene;

public class CtoCostKeywordDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOption_CostKeywordRequest>
{
	private readonly string _optionType;

	public CtoCostKeywordDebugRenderer(CastingTimeOption_CostKeywordRequest request)
		: base(request)
	{
		_optionType = EnumExtensions.EnumCleanName(request.OptionType);
	}

	public override void Render()
	{
		if (GUILayout.Button("Submit " + _optionType))
		{
			_request.SubmitKeywordAction();
		}
	}
}
