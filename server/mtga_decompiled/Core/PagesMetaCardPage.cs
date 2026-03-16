using System.Collections.Generic;
using UnityEngine;

public class PagesMetaCardPage : MonoBehaviour
{
	private readonly List<PagesMetaCardView> _cardViews = new List<PagesMetaCardView>();

	private Transform _cachedTransform;

	public List<PagesMetaCardView> CardViews => _cardViews;

	public int LastPageIndex { get; set; } = -1;

	private void Awake()
	{
		_cachedTransform = base.transform;
	}

	public void SetY(float y)
	{
		Vector3 localPosition = _cachedTransform.localPosition;
		localPosition.y = y;
		_cachedTransform.localPosition = localPosition;
	}

	public void SetX(float x)
	{
		Vector3 localPosition = _cachedTransform.localPosition;
		localPosition.x = x;
		_cachedTransform.localPosition = localPosition;
	}
}
