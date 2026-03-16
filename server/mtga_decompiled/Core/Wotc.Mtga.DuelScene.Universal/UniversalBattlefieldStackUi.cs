using System.Collections.Generic;
using Pooling;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldStackUi : MonoBehaviour
{
	[SerializeField]
	private Transform uiRoot;

	[SerializeField]
	private GameObject counterUiPrefab;

	[SerializeField]
	private GameObject expandUiPrefab;

	private List<CdcStackCounterView> countUis = new List<CdcStackCounterView>();

	private GameObject expandUi;

	public void Clear(IUnityObjectPool pool)
	{
		foreach (CdcStackCounterView countUi in countUis)
		{
			if ((bool)countUi)
			{
				countUi.Cleanup();
				countUi.parentInstanceId = 0u;
				pool.PushObject(countUi.gameObject, worldPositionStays: false);
			}
		}
		countUis.Clear();
		if ((bool)expandUi)
		{
			pool.PushObject(expandUi, worldPositionStays: false);
		}
		expandUi = null;
	}

	public void AddCount(int count, IUnityObjectPool pool)
	{
		CdcStackCounterView component = pool.PopObject(counterUiPrefab).GetComponent<CdcStackCounterView>();
		component.Init(null, 0u);
		component.transform.SetParent(uiRoot, worldPositionStays: false);
		component.SetCount(count);
		countUis.Add(component);
	}

	public void SetExpandEnabled(IUnityObjectPool pool)
	{
		if (!expandUi)
		{
			expandUi = pool.PopObject(expandUiPrefab);
			expandUi.transform.SetParent(uiRoot, worldPositionStays: false);
		}
	}
}
