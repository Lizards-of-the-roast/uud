using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

public class CombatIcon_BlockerFrame : MonoBehaviour
{
	[Serializable]
	public class DynamicObjectData
	{
		public GameObject Object;

		public List<CombatBlockState> ActiveInStates = new List<CombatBlockState>();
	}

	[SerializeField]
	private List<DynamicObjectData> _dynamicObjects = new List<DynamicObjectData>();

	public void SetupForState(CombatBlockState state)
	{
		foreach (DynamicObjectData dynamicObject in _dynamicObjects)
		{
			GameObject gameObject = dynamicObject.Object;
			if (!(gameObject == null))
			{
				bool flag = dynamicObject.ActiveInStates.Contains(state);
				if (gameObject.activeSelf != flag)
				{
					gameObject.SetActive(flag);
				}
			}
		}
	}
}
