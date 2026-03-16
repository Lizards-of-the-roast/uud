using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CDCSmartAnchor : MonoBehaviour
{
	[SerializeField]
	private AnchorPointType _anchorType;

	[SerializeField]
	private List<SmartAnchorRelationship> _anchorRelationships;

	[SerializeField]
	private int _layoutPriority;

	[SerializeField]
	private bool _usePartBounds;

	public AnchorPointType AnchorType => _anchorType;

	public List<SmartAnchorRelationship> AnchorRelationships => _anchorRelationships;

	public int LayoutPriority => _layoutPriority;

	public bool UsePartBounds => _usePartBounds;
}
