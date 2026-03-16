using UnityEngine;
using Wotc.Mtga.Unity;

public class ToSpellStackFromEntityIntention : ToEntityFromEntityIntention
{
	protected override void OnArrowBehaviorSet()
	{
		base.OnArrowBehaviorSet();
		if ((bool)base.ArrowBehavior)
		{
			float num = ((DuelScene_CDC)_endEntityView).ActiveScaffold.GetColliderBounds.size.y * 0.5f;
			base.ArrowBehavior.SetEnd(_endTransform, new Vector3(0f, 0f - num, 0f), DreamteckIntentionArrowBehavior.Space.Local);
			base.ArrowBehavior.SetCamera(CurrentCamera.Value, 22.75f);
			base.ArrowBehavior.PreferredArcDirection = -CurrentCamera.Value.gameObject.transform.up;
			base.ArrowBehavior.Roundness = 1f;
		}
	}
}
