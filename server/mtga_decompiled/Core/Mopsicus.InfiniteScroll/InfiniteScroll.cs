using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mopsicus.InfiniteScroll;

public class InfiniteScroll : MonoBehaviour
{
	public delegate int HeightItem(int index);

	public delegate int WidthtItem(int index);

	private int _prevItemIndex = -1;

	public Action<int, GameObject> OnFill = delegate
	{
	};

	[Header("Item settings")]
	public GameObject Prefab;

	[Header("Padding")]
	public int TopPadding = 10;

	public int BottomPadding = 10;

	[Header("Padding")]
	public int LeftPadding = 10;

	public int RightPadding = 10;

	public int ItemSpacing = 2;

	[HideInInspector]
	public int Type;

	private ScrollRect _scroll;

	private RectTransform _content;

	private Rect _container;

	private RectTransform[] _rects;

	private GameObject[] _views;

	private int _previousPosition = -1;

	private int _count;

	private Dictionary<int, int> _heights;

	private Dictionary<int, int> _widths;

	private Dictionary<int, float> _positions;

	public event HeightItem OnHeight;

	public event HeightItem OnWidth;

	private void Awake()
	{
		_container = GetComponent<RectTransform>().rect;
		_scroll = GetComponent<ScrollRect>();
		_content = _scroll.viewport.transform.GetChild(0).GetComponent<RectTransform>();
		_heights = new Dictionary<int, int>();
		_widths = new Dictionary<int, int>();
		_positions = new Dictionary<int, float>();
	}

	private void Update()
	{
		if (Type == 0)
		{
			UpdateVertical();
		}
		else
		{
			UpdateHorizontal();
		}
	}

	private void UpdateVertical()
	{
		if (_count == 0)
		{
			return;
		}
		float height = _content.rect.height;
		int num = (int)((_content.anchoredPosition.y - (float)ItemSpacing) / (height / (float)_count));
		if (_prevItemIndex != num)
		{
			for (int i = 0; i < _views.Length; i++)
			{
				if (num + i < 0 || num + i > _positions.Count - 1)
				{
					_views[i].SetActive(value: false);
					continue;
				}
				_views[i].SetActive(value: true);
				Vector2 anchoredPosition = _rects[i].anchoredPosition;
				anchoredPosition.y = _positions[num + i];
				_rects[i].anchoredPosition = anchoredPosition;
				_views[i].name = num.ToString();
				OnFill(num + i, _views[i]);
			}
		}
		_prevItemIndex = num;
	}

	private void UpdateHorizontal()
	{
		if (_count == 0)
		{
			return;
		}
		float num = _content.anchoredPosition.x * -1f - (float)ItemSpacing;
		if (num <= 0f && _rects[0].anchoredPosition.x < (float)(-LeftPadding) - 10f)
		{
			InitData(_count);
		}
		else
		{
			if (num < 0f)
			{
				return;
			}
			float num2 = Mathf.Abs(_positions[_previousPosition]) + (float)_widths[_previousPosition];
			int num3 = ((num > num2) ? (_previousPosition + 1) : (_previousPosition - 1));
			if (num3 < 0 || _previousPosition == num3 || _scroll.velocity.x == 0f)
			{
				return;
			}
			if (num3 > _previousPosition)
			{
				if (num3 - _previousPosition > 1)
				{
					num3 = _previousPosition + 1;
				}
				int num4 = num3 % _views.Length;
				num4--;
				if (num4 < 0)
				{
					num4 = _views.Length - 1;
				}
				int num5 = num3 + _views.Length - 1;
				if (num5 < _count)
				{
					Vector2 anchoredPosition = _rects[num4].anchoredPosition;
					anchoredPosition.x = _positions[num5];
					_rects[num4].anchoredPosition = anchoredPosition;
					Vector2 sizeDelta = _rects[num4].sizeDelta;
					sizeDelta.x = _widths[num5];
					_rects[num4].sizeDelta = sizeDelta;
					_views[num4].name = num5.ToString();
					OnFill(num5, _views[num4]);
				}
			}
			else
			{
				if (_previousPosition - num3 > 1)
				{
					num3 = _previousPosition - 1;
				}
				int num6 = num3 % _views.Length;
				Vector2 anchoredPosition2 = _rects[num6].anchoredPosition;
				anchoredPosition2.x = _positions[num3];
				_rects[num6].anchoredPosition = anchoredPosition2;
				Vector2 sizeDelta2 = _rects[num6].sizeDelta;
				sizeDelta2.x = _widths[num3];
				_rects[num6].sizeDelta = sizeDelta2;
				_views[num6].name = num3.ToString();
				OnFill(num3, _views[num6]);
			}
			_previousPosition = num3;
		}
	}

	public void InitData(int count)
	{
		if (Type == 0)
		{
			InitVertical(count);
		}
		else
		{
			InitHorizontal(count);
		}
	}

	private void InitVertical(int count)
	{
		float y = CalcSizesPositions(count);
		CreateViews();
		_previousPosition = 0;
		_count = count;
		_content.sizeDelta = new Vector2(_content.sizeDelta.x, y);
		Vector2 anchoredPosition = _content.anchoredPosition;
		Vector2 zero = Vector2.zero;
		anchoredPosition.y = 0f;
		_content.anchoredPosition = anchoredPosition;
		int num = TopPadding;
		bool flag = false;
		for (int i = 0; i < _views.Length; i++)
		{
			flag = i < count;
			_views[i].gameObject.SetActive(flag);
			if (i + 1 <= _count)
			{
				anchoredPosition = _rects[i].anchoredPosition;
				anchoredPosition.y = _positions[i];
				anchoredPosition.x = 0f;
				_rects[i].anchoredPosition = anchoredPosition;
				zero = _rects[i].sizeDelta;
				zero.y = _heights[i];
				_rects[i].sizeDelta = zero;
				num += ItemSpacing + _heights[i];
				_views[i].name = i.ToString();
				OnFill(i, _views[i]);
			}
		}
	}

	private void InitHorizontal(int count)
	{
		float x = CalcSizesPositions(count);
		CreateViews();
		_previousPosition = 0;
		_count = count;
		_content.sizeDelta = new Vector2(x, _content.sizeDelta.y);
		Vector2 anchoredPosition = _content.anchoredPosition;
		Vector2 zero = Vector2.zero;
		anchoredPosition.x = 0f;
		_content.anchoredPosition = anchoredPosition;
		int num = LeftPadding;
		bool flag = false;
		for (int i = 0; i < _views.Length; i++)
		{
			flag = i < count;
			_views[i].gameObject.SetActive(flag);
			if (i + 1 <= _count)
			{
				anchoredPosition = _rects[i].anchoredPosition;
				anchoredPosition.x = _positions[i];
				anchoredPosition.y = 0f;
				_rects[i].anchoredPosition = anchoredPosition;
				zero = _rects[i].sizeDelta;
				zero.x = _widths[i];
				_rects[i].sizeDelta = zero;
				num += ItemSpacing + _widths[i];
				_views[i].name = i.ToString();
				OnFill(i, _views[i]);
			}
		}
	}

	private float CalcSizesPositions(int count)
	{
		if (Type != 0)
		{
			return CalcSizesPositionsHorizontal(count);
		}
		return CalcSizesPositionsVertical(count);
	}

	private float CalcSizesPositionsVertical(int count)
	{
		_heights.Clear();
		_positions.Clear();
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			_heights[i] = this.OnHeight(i);
			_positions[i] = 0f - ((float)(TopPadding + i * ItemSpacing) + num);
			num += (float)_heights[i];
		}
		return num + (float)(TopPadding + BottomPadding + ((count != 0) ? ((count - 1) * ItemSpacing) : 0));
	}

	private float CalcSizesPositionsHorizontal(int count)
	{
		_widths.Clear();
		_positions.Clear();
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			_widths[i] = this.OnWidth(i);
			_positions[i] = (float)(LeftPadding + i * ItemSpacing) + num;
			num += (float)_widths[i];
		}
		return num + (float)(LeftPadding + RightPadding + ((count != 0) ? ((count - 1) * ItemSpacing) : 0));
	}

	private void MoveDataTo(int index, float height)
	{
		if (Type == 0)
		{
			MoveDataToVertical(index, height);
		}
		else
		{
			MoveDataToHorizontal(index, height);
		}
	}

	private void MoveDataToVertical(int index, float height)
	{
		_content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
		Vector2 anchoredPosition = _content.anchoredPosition;
		for (int i = 0; i < _views.Length; i++)
		{
			int num = index % _views.Length;
			_views[num].name = index.ToString();
			if (index >= _count)
			{
				_views[num].gameObject.SetActive(value: false);
				continue;
			}
			_views[num].gameObject.SetActive(value: true);
			OnFill(index, _views[num]);
			anchoredPosition = _rects[num].anchoredPosition;
			anchoredPosition.y = _positions[index];
			_rects[num].anchoredPosition = anchoredPosition;
			Vector2 sizeDelta = _rects[num].sizeDelta;
			sizeDelta.y = _heights[index];
			_rects[num].sizeDelta = sizeDelta;
			index++;
		}
	}

	private void MoveDataToHorizontal(int index, float width)
	{
		_content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
		Vector2 anchoredPosition = _content.anchoredPosition;
		for (int i = 0; i < _views.Length; i++)
		{
			int num = index % _views.Length;
			_views[num].name = index.ToString();
			if (index >= _count)
			{
				_views[num].gameObject.SetActive(value: false);
				continue;
			}
			_views[num].gameObject.SetActive(value: true);
			OnFill(index, _views[num]);
			anchoredPosition = _rects[num].anchoredPosition;
			anchoredPosition.x = _positions[index];
			_rects[num].anchoredPosition = anchoredPosition;
			Vector2 sizeDelta = _rects[num].sizeDelta;
			sizeDelta.x = _widths[index];
			_rects[num].sizeDelta = sizeDelta;
			index++;
		}
	}

	public void RecycleAll()
	{
		_count = 0;
		if (_views != null)
		{
			for (int i = 0; i < _views.Length; i++)
			{
				_views[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void Recycle(int index)
	{
		_count--;
		string strB = index.ToString();
		float height = CalcSizesPositions(_count);
		for (int i = 0; i < _views.Length; i++)
		{
			if (string.CompareOrdinal(_views[i].name, strB) == 0)
			{
				_views[i].gameObject.SetActive(value: false);
				MoveDataTo(i, height);
				break;
			}
		}
	}

	public void UpdateVisible()
	{
		bool flag = false;
		for (int i = 0; i < _views.Length; i++)
		{
			flag = i < _count;
			_views[i].gameObject.SetActive(flag);
			if (i + 1 <= _count)
			{
				int arg = int.Parse(_views[i].name);
				OnFill(arg, _views[i]);
			}
		}
	}

	public void RefreshViews()
	{
		if (_views != null)
		{
			for (int i = 0; i < _views.Length; i++)
			{
				UnityEngine.Object.Destroy(_views[i].gameObject);
			}
			_rects = null;
			_views = null;
			CreateViews();
		}
	}

	private void CreateViews()
	{
		if (Type == 0)
		{
			CreateViewsVertical();
		}
		else
		{
			CreateViewsHorizontal();
		}
	}

	private void CreateViewsVertical()
	{
		if (_views != null)
		{
			return;
		}
		int num = 0;
		foreach (int value in _heights.Values)
		{
			num += value;
		}
		num = ((_heights.Count > 0) ? (num / _heights.Count) : 0);
		int num2 = ((num > 0) ? Mathf.RoundToInt(_container.height / (float)num) : 0) + 4;
		_views = new GameObject[num2];
		for (int i = 0; i < num2; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity);
			gameObject.transform.SetParent(_content);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.pivot = new Vector2(0.5f, 1f);
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = Vector2.one;
			component.offsetMax = Vector2.zero;
			component.offsetMin = Vector2.zero;
			_views[i] = gameObject;
		}
		_rects = new RectTransform[_views.Length];
		for (int j = 0; j < _views.Length; j++)
		{
			_rects[j] = _views[j].gameObject.GetComponent<RectTransform>();
		}
	}

	private void CreateViewsHorizontal()
	{
		if (_views != null)
		{
			return;
		}
		int num = 0;
		foreach (int value in _widths.Values)
		{
			num += value;
		}
		num /= _widths.Count;
		int num2 = Mathf.RoundToInt(_container.width / (float)num) + 4;
		_views = new GameObject[num2];
		for (int i = 0; i < num2; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity);
			gameObject.transform.SetParent(_content);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.pivot = new Vector2(0f, 0.5f);
			component.anchorMin = Vector2.zero;
			component.anchorMax = new Vector2(0f, 1f);
			component.offsetMax = Vector2.zero;
			component.offsetMin = Vector2.zero;
			_views[i] = gameObject;
		}
		_rects = new RectTransform[_views.Length];
		for (int j = 0; j < _views.Length; j++)
		{
			_rects[j] = _views[j].gameObject.GetComponent<RectTransform>();
		}
	}
}
