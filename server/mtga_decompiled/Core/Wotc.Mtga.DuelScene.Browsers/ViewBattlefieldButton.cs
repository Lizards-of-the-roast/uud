using System;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class ViewBattlefieldButton : MonoBehaviour
{
	private Action _onPointerUp;

	[SerializeField]
	private GameObject _buttonHintVFX;

	public void Init(Action onPointerUp, bool suppressVFX)
	{
		_onPointerUp = onPointerUp;
		_buttonHintVFX.UpdateActive(!suppressVFX);
	}

	public void Cleanup()
	{
		_onPointerUp = null;
		_buttonHintVFX.UpdateActive(active: false);
	}

	public void OnPointerUp()
	{
		_onPointerUp?.Invoke();
	}
}
