using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SlicedMesh : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer _topLeft;

	[SerializeField]
	private MeshRenderer _topCenter;

	[SerializeField]
	private MeshRenderer _topRight;

	[SerializeField]
	private MeshRenderer _middleLeft;

	[SerializeField]
	private MeshRenderer _middleCenter;

	[SerializeField]
	private MeshRenderer _middleRight;

	[SerializeField]
	private MeshRenderer _bottomLeft;

	[SerializeField]
	private MeshRenderer _bottomCenter;

	[SerializeField]
	private MeshRenderer _bottomRight;

	private Mesh _topLeftMesh;

	private Mesh _topCenterMesh;

	private Mesh _topRightMesh;

	private Mesh _midLeftMesh;

	private Mesh _midCenterMesh;

	private Mesh _midRightMesh;

	private Mesh _bottomLeftMesh;

	private Mesh _bottomCenterMesh;

	private Mesh _bottomRightMesh;

	private RectTransform _transform;

	private Bounds _slicedBounds;

	private Bounds _origTopLeft;

	private Bounds _origTopCenter;

	private Bounds _origTopRight;

	private Bounds _origMidLeft;

	private Bounds _origMidCenter;

	private Bounds _origMidRight;

	private Bounds _origBottomLeft;

	private Bounds _origBottomCenter;

	private Bounds _origBottomRight;

	private Bounds _idealTopLeft;

	private Bounds _idealTopCenter;

	private Bounds _idealTopRight;

	private Bounds _idealMidLeft;

	private Bounds _idealMidCenter;

	private Bounds _idealMidRight;

	private Bounds _idealBottomLeft;

	private Bounds _idealBottomCenter;

	private Bounds _idealBottomRight;

	public void Awake()
	{
		_transform = (RectTransform)base.transform;
	}

	private void GatherInitialBounds()
	{
		_topLeftMesh = ((!_topLeft) ? null : (_topLeftMesh ? _topLeftMesh : _topLeft.GetComponent<MeshFilter>().sharedMesh));
		_topCenterMesh = ((!_topCenter) ? null : (_topCenterMesh ? _topCenterMesh : _topCenter.GetComponent<MeshFilter>().sharedMesh));
		_topRightMesh = ((!_topRight) ? null : (_topRightMesh ? _topRightMesh : _topRight.GetComponent<MeshFilter>().sharedMesh));
		_midLeftMesh = ((!_middleLeft) ? null : (_midLeftMesh ? _midLeftMesh : _middleLeft.GetComponent<MeshFilter>().sharedMesh));
		_midCenterMesh = ((!_middleCenter) ? null : (_midCenterMesh ? _midCenterMesh : _middleCenter.GetComponent<MeshFilter>().sharedMesh));
		_midRightMesh = ((!_middleRight) ? null : (_midRightMesh ? _midRightMesh : _middleRight.GetComponent<MeshFilter>().sharedMesh));
		_bottomLeftMesh = ((!_bottomLeft) ? null : (_bottomLeftMesh ? _bottomLeftMesh : _bottomLeft.GetComponent<MeshFilter>().sharedMesh));
		_bottomCenterMesh = ((!_bottomCenter) ? null : (_bottomCenterMesh ? _bottomCenterMesh : _bottomCenter.GetComponent<MeshFilter>().sharedMesh));
		_bottomRightMesh = ((!_bottomRight) ? null : (_bottomRightMesh ? _bottomRightMesh : _bottomRight.GetComponent<MeshFilter>().sharedMesh));
		_origTopLeft = (_topLeftMesh ? _topLeftMesh.bounds : default(Bounds));
		_origTopCenter = (_topCenterMesh ? _topCenterMesh.bounds : default(Bounds));
		_origTopRight = (_topRightMesh ? _topRightMesh.bounds : default(Bounds));
		_origMidLeft = (_midLeftMesh ? _midLeftMesh.bounds : default(Bounds));
		_origMidCenter = (_midCenterMesh ? _midCenterMesh.bounds : default(Bounds));
		_origMidRight = (_midRightMesh ? _midRightMesh.bounds : default(Bounds));
		_origBottomLeft = (_bottomLeftMesh ? _bottomLeftMesh.bounds : default(Bounds));
		_origBottomCenter = (_bottomCenterMesh ? _bottomCenterMesh.bounds : default(Bounds));
		_origBottomRight = (_bottomRightMesh ? _bottomRightMesh.bounds : default(Bounds));
	}

	private void CalculateIdealBounds()
	{
		_idealTopLeft = (_idealTopCenter = (_idealTopRight = default(Bounds)));
		_idealMidLeft = (_idealMidCenter = (_idealMidRight = default(Bounds)));
		_idealBottomLeft = (_idealBottomCenter = (_idealBottomRight = default(Bounds)));
		if ((bool)_topLeft)
		{
			Vector2 vector = new Vector2(_slicedBounds.min.x + _origTopLeft.extents.x, _slicedBounds.max.y - _origTopLeft.extents.y) - (Vector2)_transform.position;
			_idealTopLeft = new Bounds(vector, _origTopLeft.size);
		}
		if ((bool)_topRight)
		{
			Vector2 vector2 = new Vector2(_slicedBounds.max.x - _origTopRight.extents.x, _slicedBounds.max.y - _origTopRight.extents.y) - (Vector2)_transform.position;
			_idealTopRight = new Bounds(vector2, _origTopRight.size);
		}
		if ((bool)_bottomLeft)
		{
			Vector2 vector3 = new Vector2(_slicedBounds.min.x + _origBottomLeft.extents.x, _slicedBounds.min.y + _origBottomLeft.extents.y) - (Vector2)_transform.position;
			_idealBottomLeft = new Bounds(vector3, _origBottomLeft.size);
		}
		if ((bool)_bottomRight)
		{
			Vector2 vector4 = new Vector2(_slicedBounds.max.x - _origBottomRight.extents.x, _slicedBounds.min.y + _origBottomRight.extents.y) - (Vector2)_transform.position;
			_idealBottomRight = new Bounds(vector4, _origBottomRight.size);
		}
		if ((bool)_topCenter)
		{
			Vector2 vector5 = new Vector2(_slicedBounds.center.x + _idealTopLeft.size.x - _idealTopRight.size.x, _slicedBounds.max.y - _origTopCenter.extents.y) - (Vector2)_transform.position;
			_idealTopCenter = new Bounds(size: new Vector2(_slicedBounds.size.x - _idealTopLeft.size.x - _idealTopRight.size.x, _origTopCenter.size.y), center: vector5);
		}
		if ((bool)_bottomCenter)
		{
			Vector2 vector6 = new Vector2(_slicedBounds.center.x + _idealBottomLeft.size.x - _idealBottomRight.size.x, _slicedBounds.min.y + _origBottomCenter.extents.y) - (Vector2)_transform.position;
			_idealBottomCenter = new Bounds(size: new Vector2(_slicedBounds.size.x - _idealBottomLeft.size.x - _idealBottomRight.size.x, _origBottomCenter.size.y), center: vector6);
		}
		if ((bool)_middleLeft)
		{
			float num = ((_idealTopLeft != default(Bounds)) ? _idealTopLeft.size.y : _idealTopCenter.size.y);
			float num2 = ((_idealBottomLeft != default(Bounds)) ? _idealBottomLeft.size.y : _idealBottomCenter.size.y);
			Vector2 vector7 = new Vector2(_slicedBounds.min.x + _origMidLeft.extents.x, _slicedBounds.center.y + num2 - num) - (Vector2)_transform.position;
			_idealMidLeft = new Bounds(size: new Vector2(_origMidLeft.size.x, _slicedBounds.size.y - num - num2), center: vector7);
		}
		if ((bool)_middleRight)
		{
			float num3 = ((_idealTopRight != default(Bounds)) ? _idealTopRight.size.y : _idealTopCenter.size.y);
			float num4 = ((_idealBottomRight != default(Bounds)) ? _idealBottomRight.size.y : _idealBottomCenter.size.y);
			Vector2 vector8 = new Vector2(_slicedBounds.max.x - _origMidRight.extents.x, _slicedBounds.center.y + num4 - num3) - (Vector2)_transform.position;
			_idealMidRight = new Bounds(size: new Vector2(_origMidRight.size.x, _slicedBounds.size.y - num3 - num4), center: vector8);
		}
		if ((bool)_middleCenter)
		{
			Vector2 vector9 = new Vector2(_slicedBounds.center.x + _idealMidLeft.size.x - _idealMidRight.size.x, _slicedBounds.center.y + _idealBottomCenter.size.y - _idealTopCenter.size.y) - (Vector2)_transform.position;
			_idealMidCenter = new Bounds(size: new Vector2(_slicedBounds.size.x - _idealMidLeft.size.x - _idealMidRight.size.x, _slicedBounds.size.y - _idealTopCenter.size.y - _idealBottomCenter.size.y), center: vector9);
		}
	}

	private void LateUpdate()
	{
		if (_transform.hasChanged)
		{
			UpdateSlices();
			_transform.hasChanged = false;
		}
	}

	private void UpdateSlices()
	{
		_slicedBounds = _transform.GetBounds(null);
		GatherInitialBounds();
		CalculateIdealBounds();
		updateTransformToBounds(_topLeft ? _topLeft.transform : null, _idealTopLeft, _origTopLeft);
		updateTransformToBounds(_topCenter ? _topCenter.transform : null, _idealTopCenter, _origTopCenter);
		updateTransformToBounds(_topRight ? _topRight.transform : null, _idealTopRight, _origTopRight);
		updateTransformToBounds(_middleLeft ? _middleLeft.transform : null, _idealMidLeft, _origMidLeft);
		updateTransformToBounds(_middleCenter ? _middleCenter.transform : null, _idealMidCenter, _origMidCenter);
		updateTransformToBounds(_middleRight ? _middleRight.transform : null, _idealMidRight, _origMidRight);
		updateTransformToBounds(_bottomLeft ? _bottomLeft.transform : null, _idealBottomLeft, _origBottomLeft);
		updateTransformToBounds(_bottomCenter ? _bottomCenter.transform : null, _idealBottomCenter, _origBottomCenter);
		updateTransformToBounds(_bottomRight ? _bottomRight.transform : null, _idealBottomRight, _origBottomRight);
		static void updateTransformToBounds(Transform tran, Bounds bounds, Bounds originalBounds)
		{
			if ((bool)tran && !(originalBounds == default(Bounds)))
			{
				Vector3 localScale = tran.localScale;
				Vector3 localPosition = tran.localPosition;
				localScale.x = bounds.size.x / originalBounds.size.x;
				localScale.y = bounds.size.y / originalBounds.size.y;
				localScale.z = 1f;
				localPosition.x = bounds.center.x - originalBounds.center.x * localScale.x;
				localPosition.y = bounds.center.y - originalBounds.center.y * localScale.y;
				localPosition.z = 0f;
				tran.localPosition = localPosition;
				tran.localScale = localScale;
			}
		}
	}

	public void OnDestroy()
	{
		_transform = null;
	}
}
