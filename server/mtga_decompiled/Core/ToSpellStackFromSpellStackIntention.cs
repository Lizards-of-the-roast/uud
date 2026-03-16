using UnityEngine;
using Wotc.Mtga.Unity;

public class ToSpellStackFromSpellStackIntention : ToEntityFromSpellStackIntention
{
	protected override void OnArrowBehaviorSet()
	{
		base.OnArrowBehaviorSet();
		if ((bool)base.ArrowBehavior)
		{
			DuelScene_CDC obj = (DuelScene_CDC)_endEntityView;
			float x = obj.ActiveScaffold.GetColliderBounds.size.x;
			float num = x * 0.5f;
			float y = obj.ActiveScaffold.GetColliderBounds.size.y * 0.5f;
			base.ArrowBehavior.SetEnd(_endTransform, new Vector3(0f - num + 0.1f * x, y, 0f), DreamteckIntentionArrowBehavior.Space.Local);
		}
	}
}
