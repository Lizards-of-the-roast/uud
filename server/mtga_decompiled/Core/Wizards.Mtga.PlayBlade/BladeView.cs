using System;
using AssetLookupTree;
using Pooling;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.PlayBlade;

public abstract class BladeView : MonoBehaviour
{
	private Action<BladeSelectionInfo> UpdateSelectionInfo;

	public abstract BladeType Type { get; }

	protected IUnityObjectPool UnityObjectPool { get; private set; }

	protected ICardBuilder<Meta_CDC> CardViewBuilder { get; private set; }

	protected AssetLookupSystem AssetLookupSystem { get; private set; }

	protected PlayBladeSignals Signals { get; private set; }

	public void Show()
	{
		base.gameObject.UpdateActive(active: true);
	}

	public void Hide()
	{
		base.gameObject.UpdateActive(active: false);
	}

	protected void UpdateSelection(BladeSelectionInfo current)
	{
		UpdateSelectionInfo?.Invoke(current);
	}

	public abstract void OnSelectionInfoUpdated(BladeSelectionInfo current);

	public virtual void Inject(IUnityObjectPool objectPool, PlayBladeSignals signals, ICardBuilder<Meta_CDC> cardViewBuilder, AssetLookupSystem assetLookupSystem)
	{
		UnityObjectPool = objectPool;
		Signals = signals;
		CardViewBuilder = cardViewBuilder;
		AssetLookupSystem = assetLookupSystem;
	}

	public void SubscribeToSelectionUpdates(Action<BladeSelectionInfo> updateSelectionInfo)
	{
		UpdateSelectionInfo = updateSelectionInfo;
	}

	public void UbSubscribeToSelectionUpdates()
	{
		UpdateSelectionInfo = null;
	}

	public abstract void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem);
}
