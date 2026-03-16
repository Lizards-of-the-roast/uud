using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.CardParts;

[ExecuteAlways]
public class ScaffoldingBase : MonoBehaviour
{
	public ScaffoldingBase InheritedScaffold;

	public List<AnchorPointType> IgnoredAnchorTypes = new List<AnchorPointType>();

	public List<string> IgnoredCustomAnchors = new List<string>();

	public bool OverrideShape;

	[SerializeField]
	private ScaffoldShape _shape = ScaffoldShape.Vertical;

	public bool OverrideColliderSize;

	public float ColliderWidth = 0.1f;

	private Bounds? ColliderSize;

	public Vector3 ClickGuardOffset = new Vector3(0f, 0f, 0.1f);

	public Vector3 ClickGuardScale = new Vector3(1.3f, 1.1f, 1f);

	[SerializeField]
	private bool forceBuildInclusion;

	public List<CDCSmartAnchor> AnchorPoints => new List<CDCSmartAnchor>(base.gameObject.GetComponentsInChildren<CDCSmartAnchor>(includeInactive: true));

	public List<CDCSmartAnchor> AllAnchorPoints
	{
		get
		{
			List<CDCSmartAnchor> list = new List<CDCSmartAnchor>(AnchorPoints);
			foreach (CDCSmartAnchor inheritedAnchor in (InheritedScaffold != null) ? InheritedScaffold.AllAnchorPoints : new List<CDCSmartAnchor>())
			{
				if (!ShouldIgnore(inheritedAnchor) && !(list.Find((CDCSmartAnchor x) => x.AnchorType == inheritedAnchor.AnchorType) != null))
				{
					list.Add(inheritedAnchor);
				}
			}
			list.Sort((CDCSmartAnchor x, CDCSmartAnchor y) => x.LayoutPriority.CompareTo(y.LayoutPriority));
			return list;
		}
	}

	public bool ForceBuildInclusion
	{
		get
		{
			return forceBuildInclusion;
		}
		set
		{
			forceBuildInclusion = value;
		}
	}

	public Bounds GetColliderBounds
	{
		get
		{
			if (InheritedScaffold == null || OverrideColliderSize)
			{
				if (!ColliderSize.HasValue)
				{
					Bounds value = default(Bounds);
					RectTransform rectTransform = base.transform as RectTransform;
					if (rectTransform != null)
					{
						value.center = rectTransform.localPosition;
						value.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, ColliderWidth);
					}
					ColliderSize = value;
				}
				return ColliderSize.Value;
			}
			return InheritedScaffold.GetColliderBounds;
		}
	}

	public ScaffoldShape Shape
	{
		get
		{
			if (InheritedScaffold == null || OverrideShape)
			{
				return _shape;
			}
			return InheritedScaffold.Shape;
		}
	}

	public bool ShouldIgnore(CDCSmartAnchor inheritedAnchor)
	{
		if (inheritedAnchor == null)
		{
			return true;
		}
		return IgnoredAnchorTypes.Contains(inheritedAnchor.AnchorType);
	}
}
