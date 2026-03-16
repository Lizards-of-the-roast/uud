using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultButtonView : MonoBehaviour
{
	[SerializeField]
	private DefaultButtonType _type;

	[SerializeField]
	private AnchorPosAnimationHandler _slideAnimation;

	[SerializeField]
	private Graphic _slideGraphic;

	private static readonly Dictionary<DefaultButtonType, Color> COLOR_TABLE = new Dictionary<DefaultButtonType, Color>
	{
		{
			DefaultButtonType.Green,
			new Color(0.05f, 1f, 0f, 1f)
		},
		{
			DefaultButtonType.Yellow,
			new Color(1f, 0.89f, 0f, 1f)
		},
		{
			DefaultButtonType.Red,
			new Color(1f, 0.05f, 0f, 1f)
		},
		{
			DefaultButtonType.Cyan,
			new Color(0.557f, 0.89f, 0.992f, 1f)
		}
	};

	private void Awake()
	{
		float x = (base.transform as RectTransform).rect.size.x;
		Vector2 vector = new Vector2(x, 0f);
		_slideAnimation.Disabled.Value = -vector;
		_slideAnimation.MouseOff.Value = -vector;
		_slideAnimation.MouseOver.Value = vector;
		_slideAnimation.PressedOver.Value = vector;
		_slideAnimation.PressedOff.Value = -vector;
		_slideGraphic.color = COLOR_TABLE[_type];
	}
}
