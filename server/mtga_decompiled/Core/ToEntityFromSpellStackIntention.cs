using UnityEngine;
using Wotc.Mtga.DuelScene;

public class ToEntityFromSpellStackIntention : FromSpellStackIntentionBase
{
	protected ICardViewProvider _cardViewProvider;

	protected IEntityView _endEntityView;

	protected Transform _endTransform;

	public override bool ArrowStillValid
	{
		get
		{
			if (base.ArrowStillValid && _endEntityView != null && _endEntityView.InstanceId != 0 && (bool)_endTransform)
			{
				return _endTransform.gameObject.activeSelf;
			}
			return false;
		}
	}

	public uint EndEntityId => _endEntityView?.InstanceId ?? 0;

	public virtual FromEntityIntentionBase Init(IEntityView startEntityView, IEntityView endEntityView, uint group = 0u, uint groupCount = 1u)
	{
		base.Init(startEntityView, group, groupCount);
		_endEntityView = endEntityView;
		return this;
	}

	public virtual FromEntityIntentionBase Init(IEntityView startEntityView, IEntityView endEntityView, ICardViewProvider cardViewProvider, uint group = 0u, uint groupCount = 1u)
	{
		Init(startEntityView, endEntityView, group, groupCount);
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
		_endTransform = _endEntityView.ArrowRoot;
		if (FromEntityIntentionBase.IsEntityPagedOut(_endEntityView, _cardViewProvider, out var cardView, out var region))
		{
			if (region.IsPagedLeft(cardView))
			{
				_endTransform = region.LeftPagingButton.transform;
			}
			else if (region.IsPagedRight(cardView))
			{
				_endTransform = region.RightPagingButton.transform;
			}
		}
		base.ArrowBehavior.SetEnd(_endTransform);
	}
}
