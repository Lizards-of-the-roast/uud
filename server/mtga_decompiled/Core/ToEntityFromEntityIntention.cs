using UnityEngine;
using Wotc.Mtga.DuelScene;

public class ToEntityFromEntityIntention : FromEntityIntentionBase
{
	protected ICardViewProvider _cardViewProvider;

	protected IEntityView _endEntityView;

	protected Transform _endTransform;

	public override bool ArrowStillValid
	{
		get
		{
			bool arrowStillValid = base.ArrowStillValid;
			bool flag = _endEntityView != null && _endEntityView.InstanceId != 0 && (bool)_endTransform && _endTransform.gameObject.activeSelf;
			return arrowStillValid && flag;
		}
	}

	public uint EndEntityId => _endEntityView?.InstanceId ?? 0;

	public FromEntityIntentionBase Init(IEntityView startEntityView, IEntityView endEntityView)
	{
		base.Init(startEntityView);
		_endEntityView = endEntityView;
		return this;
	}

	public virtual FromEntityIntentionBase Init(IEntityView startEntityView, IEntityView endEntityView, ICardViewProvider cardViewProvider)
	{
		Init(startEntityView, endEntityView);
		_cardViewProvider = cardViewProvider;
		return this;
	}

	public override void OnPooled()
	{
		base.OnPooled();
		_cardViewProvider = null;
		_endEntityView = null;
		_endTransform = null;
	}

	protected override void OnArrowBehaviorSet()
	{
		base.OnArrowBehaviorSet();
		if (!base.ArrowBehavior)
		{
			return;
		}
		if (FromEntityIntentionBase.IsEntityPagedOut(_startEntityView, _cardViewProvider, out var cardView, out var region))
		{
			if (region.IsPagedLeft(cardView))
			{
				_startTransform = region.LeftPagingButton.transform;
				base.ArrowBehavior.SetStart(_startTransform);
			}
			else if (region.IsPagedRight(cardView))
			{
				_startTransform = region.RightPagingButton.transform;
				base.ArrowBehavior.SetStart(_startTransform);
			}
		}
		_endTransform = _endEntityView.ArrowRoot;
		if (FromEntityIntentionBase.IsEntityPagedOut(_endEntityView, _cardViewProvider, out var cardView2, out var region2))
		{
			if (region2.IsPagedLeft(cardView2))
			{
				_endTransform = region2.LeftPagingButton.transform;
			}
			else if (region2.IsPagedRight(cardView2))
			{
				_endTransform = region2.RightPagingButton.transform;
			}
		}
		base.ArrowBehavior.SetEnd(_endTransform);
	}
}
