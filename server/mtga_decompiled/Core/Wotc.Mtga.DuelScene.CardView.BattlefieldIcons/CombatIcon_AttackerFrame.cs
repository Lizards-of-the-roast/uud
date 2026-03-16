using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

public class CombatIcon_AttackerFrame : MonoBehaviour
{
	[Serializable]
	public class DynamicObjectData
	{
		public GameObject Object;

		public CombatAttackState[] ActiveIf;
	}

	[SerializeField]
	private List<DynamicObjectData> _dynamicObjects = new List<DynamicObjectData>();

	public void SetupForState(CombatAttackState state)
	{
		foreach (DynamicObjectData dynamicObject in _dynamicObjects)
		{
			GameObject gameObject = dynamicObject.Object;
			if (!(gameObject == null))
			{
				bool flag = ((IReadOnlyCollection<CombatAttackState>)(object)dynamicObject.ActiveIf).Contains(state);
				if (gameObject.activeSelf != flag)
				{
					gameObject.SetActive(flag);
				}
			}
		}
	}
}
