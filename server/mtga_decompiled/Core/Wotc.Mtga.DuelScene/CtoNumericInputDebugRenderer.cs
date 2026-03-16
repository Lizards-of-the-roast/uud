using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CtoNumericInputDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOption_NumericInputRequest>
{
	private uint _xValue;

	public CtoNumericInputDebugRenderer(CastingTimeOption_NumericInputRequest request)
		: base(request)
	{
	}

	public override void Render()
	{
		GUILayout.BeginHorizontal();
		GUI.enabled = _xValue >= _request.StepSize;
		if (GUILayout.Button("<", GUILayout.ExpandWidth(expand: false)))
		{
			_xValue -= _request.StepSize;
		}
		GUI.enabled = true;
		uint.TryParse(GUILayout.TextField(_xValue.ToString()), out _xValue);
		GUI.enabled = _xValue <= _request.Max - _request.StepSize;
		if (GUILayout.Button(">", GUILayout.ExpandWidth(expand: false)))
		{
			_xValue += _request.StepSize;
		}
		GUI.enabled = true;
		GUILayout.Space(10f);
		if (GUILayout.Button("Min", GUILayout.ExpandWidth(expand: false)))
		{
			_xValue = _request.Min;
		}
		if (GUILayout.Button("Max", GUILayout.ExpandWidth(expand: false)))
		{
			_xValue = _request.Max;
		}
		GUILayout.EndHorizontal();
		if (GUILayout.Button($"Submit X = {_xValue}"))
		{
			_request.SubmitX(_xValue);
		}
	}
}
