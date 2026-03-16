using System.Collections.Generic;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class CDCPart_Counters_Battlefield : CDCPart_Counters
{
	[Range(0.1f, 10f)]
	[SerializeField]
	private float _counterSize = 1f;

	[SerializeField]
	protected GameObject _readMorePrefab;

	private GameObject _readMoreIcon;

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		RemoveExtrasAndShowReadMore();
	}

	private void RemoveExtrasAndShowReadMore()
	{
		List<CounterType> active = _diffOutput.Active;
		int count = active.Count;
		IObjectPool objectPool = Pantry.Get<IObjectPool>();
		List<ViewCounter> list = objectPool.PopObject<List<ViewCounter>>() ?? new List<ViewCounter>();
		for (int i = 0; i < count; i++)
		{
			if (_activeCounters.TryGetValue(GetVisibleKey(active[i]), out var value) && !list.Contains(value))
			{
				list.Add(value);
			}
		}
		int num = MaxVisible(list.Count);
		for (int j = 0; j < list.Count; j++)
		{
			list[j].gameObject.SetActive(j < num);
		}
		GameObject obj = ReadMoreIcon();
		obj.UpdateActive(list.Count > num);
		obj.transform.SetAsLastSibling();
		objectPool.PushObject(list);
	}

	private GameObject ReadMoreIcon()
	{
		if (!_readMoreIcon)
		{
			_readMoreIcon = _unityObjectPool.PopObject(_readMorePrefab);
			_readMoreIcon.transform.SetParent(_counterRoot.transform, worldPositionStays: false);
			_readMoreIcon.transform.ZeroOut();
			_readMoreIcon.transform.SetAsLastSibling();
		}
		return _readMoreIcon;
	}

	private int MaxVisible(int counterViewCount)
	{
		float height = (_counterRoot.transform as RectTransform).rect.height;
		int num = (int)(height / _counterSize);
		if (counterViewCount <= num)
		{
			return num;
		}
		float height2 = (_readMorePrefab.transform as RectTransform).rect.height;
		return Mathf.FloorToInt((height - height2) / _counterSize);
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		if (_unityObjectPool != null && (bool)_readMoreIcon)
		{
			_unityObjectPool.PushObject(_readMoreIcon);
			_readMoreIcon = null;
		}
	}
}
