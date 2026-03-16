using System;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Interactions;

public class OnGUIEventHandler : MonoBehaviour
{
	public Action Event;

	private void OnGUI()
	{
		Event?.Invoke();
	}

	private void OnDestroy()
	{
		Event = null;
	}
}
