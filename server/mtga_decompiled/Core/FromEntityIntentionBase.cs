using System;
using UnityEngine;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Battlefield;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Unity;

public abstract class FromEntityIntentionBase : IDisposable
{
	protected IEntityView _startEntityView;

	protected Transform _startTransform;

	protected bool _arrowVisibleInternal = true;

	private DreamteckIntentionArrowBehavior _arrowBehavior;

	private bool _arrowVisibleExternal = true;

	public DreamteckIntentionArrowBehavior ArrowBehavior
	{
		get
		{
			return _arrowBehavior;
		}
		set
		{
			_arrowBehavior = value;
			OnArrowBehaviorSet();
			if ((bool)_arrowBehavior)
			{
				_arrowBehavior.Flush();
			}
		}
	}

	public virtual bool ArrowStillValid
	{
		get
		{
			if (_startEntityView != null && _startEntityView.InstanceId != 0 && (bool)_startTransform)
			{
				return _startTransform.gameObject.activeSelf;
			}
			return false;
		}
	}

	public uint SourceEntityId => _startEntityView?.InstanceId ?? 0;

	public bool Disposed { get; private set; }

	public virtual FromEntityIntentionBase Init(IEntityView startEntityView)
	{
		_startEntityView = startEntityView;
		_startTransform = null;
		ArrowBehavior = null;
		_arrowVisibleExternal = true;
		_arrowVisibleInternal = true;
		return this;
	}

	public virtual void OnPooled()
	{
		_startEntityView = null;
		_startTransform = null;
		ArrowBehavior = null;
		_arrowVisibleExternal = true;
		_arrowVisibleInternal = true;
	}

	public virtual void UpdateArrow()
	{
		ArrowBehavior.gameObject.UpdateActive(_arrowVisibleInternal && _arrowVisibleExternal);
	}

	public void SetArrowVisible(bool isVisible)
	{
		if (_arrowVisibleExternal != isVisible)
		{
			_arrowVisibleExternal = isVisible;
			UpdateArrow();
		}
	}

	protected virtual void OnArrowBehaviorSet()
	{
		if ((bool)_arrowBehavior)
		{
			_arrowBehavior.SetCamera(null);
			_arrowBehavior.PreferredArcDirection = Vector3.up;
			_arrowBehavior.Roundness = 0.333f;
			_startTransform = _startEntityView.ArrowRoot;
			_arrowBehavior.SetStart(_startTransform);
		}
	}

	protected static bool IsEntityPagedOut(IEntityView entityView, ICardViewProvider cardViewProvider, out DuelScene_CDC cardView, out BattlefieldRegion region)
	{
		cardView = null;
		region = null;
		if (cardViewProvider != null && (object)(cardView = cardViewProvider.GetCardView(entityView.InstanceId)) != null && cardView.CurrentCardHolder is BattlefieldCardHolder battlefieldCardHolder && (region = battlefieldCardHolder.GetRegionForCard(cardView)) != null)
		{
			return !region.IsInVisibleStack(cardView);
		}
		return false;
	}

	public void Dispose()
	{
		OnDispose(disposing: true);
		GC.SuppressFinalize(this);
		Disposed = true;
	}

	~FromEntityIntentionBase()
	{
		OnDispose(disposing: false);
	}

	protected virtual void OnDispose(bool disposing)
	{
	}
}
