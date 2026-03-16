using UnityEngine;

namespace StatsMonitor.Core;

public abstract class View2D
{
	internal GameObject gameObject;

	private RectTransform _rectTransform;

	public RectTransform RTransform
	{
		get
		{
			if (_rectTransform != null)
			{
				return _rectTransform;
			}
			_rectTransform = gameObject.GetComponent<RectTransform>();
			if (_rectTransform == null)
			{
				_rectTransform = gameObject.AddComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public float Width
	{
		get
		{
			return RTransform.rect.width;
		}
		set
		{
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
		}
	}

	public float Height
	{
		get
		{
			return RTransform.rect.height;
		}
		set
		{
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
		}
	}

	public float X
	{
		get
		{
			return RTransform.anchoredPosition.x;
		}
		set
		{
			RTransform.anchoredPosition = new Vector2(value, Y);
		}
	}

	public float Y
	{
		get
		{
			return RTransform.anchoredPosition.y;
		}
		set
		{
			RTransform.anchoredPosition = new Vector2(X, value);
		}
	}

	public Vector2 Pivot
	{
		get
		{
			return RTransform.pivot;
		}
		set
		{
			RTransform.pivot = value;
		}
	}

	public Vector2 AnchorMin
	{
		get
		{
			return RTransform.anchorMin;
		}
		set
		{
			RTransform.anchorMin = value;
		}
	}

	public Vector2 AnchorMax
	{
		get
		{
			return RTransform.anchorMax;
		}
		set
		{
			RTransform.anchorMax = value;
		}
	}

	public void SetPosition(float x, float y)
	{
		RTransform.anchoredPosition = new Vector2(x, y);
	}

	public void SetSize(float width, float height)
	{
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

	public void SetScale(float h = 1f, float v = 1f)
	{
		RTransform.localScale = new Vector3(h, v, 1f);
	}

	public void SetPivotAndAnchor(Vector2 vector)
	{
		Vector2 vector2 = (AnchorMax = vector);
		Vector2 pivot = (AnchorMin = vector2);
		Pivot = pivot;
	}

	public void SetRTransformValues(float x, float y, float width, float height, Vector2 pivotAndAnchor)
	{
		RTransform.anchoredPosition = new Vector2(x, y);
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		Vector2 vector = (AnchorMax = pivotAndAnchor);
		Vector2 pivot = (AnchorMin = vector);
		Pivot = pivot;
	}

	public void Invalidate(ViewInvalidationType type = ViewInvalidationType.All)
	{
		if (gameObject == null)
		{
			gameObject = CreateChildren();
		}
		RTransform.anchoredPosition3D = new Vector3(RTransform.anchoredPosition.x, RTransform.anchoredPosition.y, 0f);
		SetScale();
		if (type == ViewInvalidationType.Style || type == ViewInvalidationType.All)
		{
			UpdateStyle();
		}
		if (type == ViewInvalidationType.Layout || type == ViewInvalidationType.All)
		{
			UpdateLayout();
		}
	}

	public virtual void Reset()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Dispose()
	{
		Destroy(gameObject);
		gameObject = null;
	}

	internal static void Destroy(Object obj)
	{
		Object.Destroy(obj);
	}

	protected virtual GameObject CreateChildren()
	{
		return null;
	}

	protected virtual void UpdateStyle()
	{
	}

	protected virtual void UpdateLayout()
	{
	}
}
