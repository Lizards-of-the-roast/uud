using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScale : ButtonModifier
{
	[Serializable]
	private struct StateScaleInfo
	{
		public ButtonStateModifier.ButtonState state;

		public Vector3 scale;
	}

	[SerializeField]
	private List<StateScaleInfo> stateScales;

	[SerializeField]
	private List<Transform> targetTransforms;

	private Dictionary<ButtonStateModifier.ButtonState, Vector3> stateToScale;

	private void Awake()
	{
		stateToScale = new Dictionary<ButtonStateModifier.ButtonState, Vector3>();
		foreach (StateScaleInfo stateScale in stateScales)
		{
			stateToScale.Add(stateScale.state, stateScale.scale);
		}
	}

	public override void UpdateModifier(ButtonStateModifier.ButtonState buttonState)
	{
		foreach (Transform targetTransform in targetTransforms)
		{
			if (!(targetTransform == null))
			{
				targetTransform.localScale = stateToScale[buttonState];
			}
		}
	}
}
