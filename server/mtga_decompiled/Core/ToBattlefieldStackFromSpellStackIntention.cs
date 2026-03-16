using UnityEngine;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Unity;

public class ToBattlefieldStackFromSpellStackIntention : ToEntityFromSpellStackIntention
{
	private IBattlefieldCardHolder _battlefieldCardHolder;

	private Transform _counterTransform;

	public override bool ArrowStillValid
	{
		get
		{
			if (!base.ArrowStillValid)
			{
				if ((bool)_counterTransform)
				{
					return IsBuriedOffscreen;
				}
				return false;
			}
			return true;
		}
	}

	private bool IsBuriedOffscreen
	{
		get
		{
			bool result = false;
			DuelScene_CDC duelScene_CDC = (DuelScene_CDC)_endEntityView;
			if (_battlefieldCardHolder.GetStackForCard(duelScene_CDC) is BattlefieldCardHolder.BattlefieldStack battlefieldStack && battlefieldStack.AllCards.IndexOf(duelScene_CDC) > 3)
			{
				result = true;
			}
			return result;
		}
	}

	public virtual FromEntityIntentionBase Init(IEntityView startEntityView, IEntityView endEntityView, IBattlefieldCardHolder battlefieldCardHolder, ICardViewProvider cardViewProvider, uint group = 0u, uint groupCount = 1u)
	{
		base.Init(startEntityView, endEntityView, cardViewProvider, group, groupCount);
		_battlefieldCardHolder = battlefieldCardHolder;
		return this;
	}

	public override void OnPooled()
	{
		base.OnPooled();
		_battlefieldCardHolder = null;
		_counterTransform = null;
	}

	protected override void OnArrowBehaviorSet()
	{
		base.OnArrowBehaviorSet();
		if (!base.ArrowBehavior)
		{
			return;
		}
		DuelScene_CDC duelScene_CDC = (DuelScene_CDC)_endEntityView;
		if (IsBuriedOffscreen && _endTransform == _endEntityView.ArrowRoot)
		{
			IBattlefieldStack stackForCard = _battlefieldCardHolder.GetStackForCard(duelScene_CDC);
			foreach (CdcStackCounterView activeStackCounter in ((BattlefieldCardHolder.BattlefieldLayout)_battlefieldCardHolder.Layout).ActiveStackCounters)
			{
				if (activeStackCounter.parentInstanceId == stackForCard.StackParentModel.InstanceId)
				{
					_counterTransform = activeStackCounter.transform;
					break;
				}
			}
			base.ArrowBehavior.SetEnd(_counterTransform, Vector3.zero, DreamteckIntentionArrowBehavior.Space.Local);
		}
		else if (_endTransform == _endEntityView.ArrowRoot)
		{
			float x = duelScene_CDC.ActiveScaffold.GetColliderBounds.size.x;
			float num = x * 0.5f;
			float y = duelScene_CDC.ActiveScaffold.GetColliderBounds.size.y * 0.5f;
			base.ArrowBehavior.SetEnd(_endTransform, new Vector3(0f - num + 0.05f * x, y, 0f), DreamteckIntentionArrowBehavior.Space.Local);
		}
	}

	protected override void OnDispose(bool disposing)
	{
		if (!base.Disposed && disposing)
		{
			_counterTransform = null;
			_battlefieldCardHolder = null;
		}
		base.OnDispose(disposing);
	}
}
