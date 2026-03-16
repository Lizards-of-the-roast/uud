using System;
using MovementSystem;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class DebugSettingsModule : DebugModule
{
	private readonly ISplineMovementSystem _splineMovementSystem;

	public override string Name => "Debug Settings";

	public override string Description => "Exposes controls for various experimental features";

	public DebugSettingsModule(ISplineMovementSystem splineMovementSystem)
	{
		_splineMovementSystem = splineMovementSystem;
	}

	public override void Render()
	{
		RenderSpeedControls();
		GUILayout.Space(3f);
		RenderDebugSettings();
		GUILayout.Space(3f);
		RenderDebugCameraControls();
	}

	private void RenderDebugSettings()
	{
		RenderEditorSettings();
		RenderTextSettings();
	}

	private void RenderEditorSettings()
	{
	}

	private void RenderTextSettings()
	{
		bool fixedRulesTextSize = MDNPlayerPrefs.FixedRulesTextSize;
		GUILayout.BeginHorizontal();
		fixedRulesTextSize = GUILayout.Toggle(fixedRulesTextSize, "Show larger text in card textboxes", GUILayout.ExpandHeight(expand: false), GUILayout.ExpandWidth(expand: false));
		GUILayout.Label("Show enlarged text in card textboxes");
		GUILayout.EndHorizontal();
		if (MDNPlayerPrefs.FixedRulesTextSize != fixedRulesTextSize)
		{
			MDNPlayerPrefs.FixedRulesTextSize = fixedRulesTextSize;
		}
	}

	private void RenderDebugCameraControls()
	{
	}

	private void RenderSpeedControls()
	{
		GUILayout.BeginHorizontal();
		float num = _splineMovementSystem.GetSpeedModifier();
		if (GUILayout.Button("-", GUILayout.Width(50f)))
		{
			if (num >= 0.5f)
			{
				num = Math.Max(num - 0.5f, 0.25f);
			}
			else if (num >= 0.25f)
			{
				num = 0.1f;
			}
			_splineMovementSystem.SetSpeedModifier(num);
		}
		GUILayout.FlexibleSpace();
		GUILayout.Label($"SPEED MOD: {num}");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+", GUILayout.Width(50f)))
		{
			if (num >= 0.5f)
			{
				num = Math.Min(num + 0.5f, 10f);
			}
			else if (num >= 0.25f)
			{
				num = 0.5f;
			}
			else if (num >= 0.1f)
			{
				num = 0.25f;
			}
			_splineMovementSystem.SetSpeedModifier(num);
		}
		GUILayout.EndHorizontal();
	}
}
