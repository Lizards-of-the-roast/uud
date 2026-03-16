using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI;

public class PlayerInfoRotationIndicator : MonoBehaviour
{
	[SerializeField]
	private List<Image> _indicators = new List<Image>();

	[SerializeField]
	private Sprite _activeIndicatorImage;

	[SerializeField]
	private Sprite _inactiveIndicatorImage;

	private int _currentIndex;

	private int _numberOfIndicators;

	public void Init(int numberOfBars)
	{
		if (numberOfBars > 0)
		{
			base.gameObject.SetActive(value: true);
			for (int i = 0; i < numberOfBars; i++)
			{
				_indicators[i].gameObject.SetActive(value: true);
			}
			_indicators[0].sprite = _activeIndicatorImage;
			_numberOfIndicators = numberOfBars;
		}
	}

	public void Rotate()
	{
		if (_numberOfIndicators > 0)
		{
			_indicators[_currentIndex++].sprite = _inactiveIndicatorImage;
			_currentIndex %= _numberOfIndicators;
			_indicators[_currentIndex].sprite = _activeIndicatorImage;
		}
	}
}
