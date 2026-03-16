using System;
using AssetLookupTree;
using Pooling;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.PlayBlade;

public abstract class BladeContentView : MonoBehaviour
{
	private Action<BladeSelectionInfo> UpdateSelectionInfo;

	protected Action OnUpdate;

	public abstract BladeType Type { get; }

	protected IUnityObjectPool UnityObjectPool { get; private set; }

	protected ICardBuilder<Meta_CDC> CardViewBuilder { get; private set; }

	protected AssetLookupSystem AssetLookupSystem { get; private set; }

	protected PlayBladeSignals Signals { get; private set; }

	public void Awake()
	{
		OnAwakeCalled();
	}

	public void OnDestroy()
	{
		OnDestroyCalled();
	}

	public void TickUpdate()
	{
		OnUpdate?.Invoke();
	}

	protected abstract void OnAwakeCalled();

	protected abstract void OnDestroyCalled();

	public virtual void Show()
	{
		base.gameObject.UpdateActive(active: true);
	}

	public virtual void Hide()
	{
		base.gameObject.UpdateActive(active: false);
	}

	protected void UpdateSelection(BladeSelectionInfo selectionInfo)
	{
		UpdateSelectionInfo?.Invoke(selectionInfo);
	}

	public abstract void OnSelectionInfoUpdated(BladeSelectionInfo selectionInfo);

	public abstract void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem);

	public void Inject(IUnityObjectPool objectPool, PlayBladeSignals signals, ICardBuilder<Meta_CDC> cardViewBuilder, AssetLookupSystem assetLookupSystem)
	{
		UnityObjectPool = objectPool;
		CardViewBuilder = cardViewBuilder;
		Signals = signals;
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
}
