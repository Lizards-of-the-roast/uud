using UnityEngine;
using Wotc.Mtga.Unity;

public abstract class FromSpellStackIntentionBase : FromEntityIntentionBase
{
	public uint Group { get; private set; }

	public uint GroupCount { get; private set; } = 1u;

	public virtual FromEntityIntentionBase Init(IEntityView startEntityView, uint group = 0u, uint groupCount = 1u)
	{
		base.Init(startEntityView);
		Group = group;
		GroupCount = groupCount;
		return this;
	}

	public override void OnPooled()
	{
		base.OnPooled();
		Group = 0u;
		GroupCount = 1u;
	}

	protected override void OnArrowBehaviorSet()
	{
		base.OnArrowBehaviorSet();
		if ((bool)base.ArrowBehavior)
		{
			DuelScene_CDC obj = (DuelScene_CDC)_startEntityView;
			float x = obj.ActiveScaffold.GetColliderBounds.size.x;
			float num = x * 0.5f;
			float y = obj.ActiveScaffold.GetColliderBounds.size.y * 0.5f;
			Vector3 startOffset = new Vector3(0f - num + x / (float)(GroupCount + 1) * (float)(Group + 1), y, 0f);
			base.ArrowBehavior.SetStart(_startTransform, startOffset, DreamteckIntentionArrowBehavior.Space.Local);
			base.ArrowBehavior.SetCamera(CurrentCamera.Value, 22.75f);
			base.ArrowBehavior.PreferredArcDirection = CurrentCamera.Value.gameObject.transform.up;
			base.ArrowBehavior.Roundness = 1f;
		}
	}
}
