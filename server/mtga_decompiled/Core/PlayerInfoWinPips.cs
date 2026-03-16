using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.UI;

public class PlayerInfoWinPips : MonoBehaviour, IPlayerInfoSlotItem
{
	[SerializeField]
	private List<Image> _pips;

	public void FadeOut(float duration)
	{
		foreach (Image pip in _pips)
		{
			pip.CrossFadeAlpha(0f, duration, ignoreTimeScale: false);
		}
	}

	public void FadeIn(float duration)
	{
		foreach (Image pip in _pips)
		{
			pip.CrossFadeAlpha(1f, duration, ignoreTimeScale: false);
		}
	}
}
