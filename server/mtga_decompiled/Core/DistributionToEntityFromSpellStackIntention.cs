using UnityEngine;
using Wotc.Mtga.Unity;

public class DistributionToEntityFromSpellStackIntention : ToEntityFromSpellStackIntention
{
	protected override void OnArrowBehaviorSet()
	{
		base.OnArrowBehaviorSet();
		if ((bool)base.ArrowBehavior && _endEntityView is DuelScene_CDC duelScene_CDC && _endTransform == _endEntityView.ArrowRoot)
		{
			float y = duelScene_CDC.ActiveScaffold.GetColliderBounds.size.y * 0.5f;
			base.ArrowBehavior.SetEnd(_endTransform, new Vector3(0f, y, 0f), DreamteckIntentionArrowBehavior.Space.Local);
		}
	}
}
