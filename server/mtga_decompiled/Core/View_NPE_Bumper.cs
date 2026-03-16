using System;
using System.Collections.Generic;
using UnityEngine;

public class View_NPE_Bumper : View_NPE_TextWidget
{
	[Serializable]
	public class BumperPosition
	{
		public PromptType Type;

		public Vector2 AnchoredPosition;
	}

	[SerializeField]
	private List<BumperPosition> _bumperPositions;

	private RectTransform _rectTrans;

	private void Awake()
	{
		_rectTrans = GetComponent<RectTransform>();
	}

	public void Show(string text, PromptType type = PromptType.Button)
	{
		base.Show(text);
		foreach (BumperPosition bumperPosition in _bumperPositions)
		{
			if (bumperPosition.Type == type)
			{
				_rectTrans.anchoredPosition = bumperPosition.AnchoredPosition;
			}
		}
	}
}
