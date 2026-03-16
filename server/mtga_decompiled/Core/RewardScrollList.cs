using System;
using System.Collections.Generic;
using Core.Meta.Shared;
using UnityEngine;
using UnityEngine.UI;

public class RewardScrollList : MonoBehaviour
{
	public Action<TransformAndIndex> transformOnScreen;

	public Action<TransformAndIndex> transformOffScreen;

	public Action layoutUpdateComplete;

	[SerializeField]
	private OptionalInteractScrollRect _scrollRect;

	[SerializeField]
	private GameObject _mockReward;

	public int Rows = 1;

	[SerializeField]
	private float _xSpaceBetweenRewards;

	[SerializeField]
	private float _ySpaceBetweenRewards;

	[Tooltip("This is how frequently the pool will attempt to load offscreen rewards in order to increase scroll performance")]
	[SerializeField]
	protected float _offscreenLoadPulse = 0.1f;

	[SerializeField]
	private float _paddingX;

	[SerializeField]
	private float _bufferMultiplier;

	[SerializeField]
	private float _maxVelocity;

	private List<Transform> _rewardTransforms;

	private int _totalRewards;

	private bool _manualScrollDragAllowed;

	private RectTransform _rectTransform;

	private Rect _lastRect;

	private bool _rewardsDirty;

	private float _rewardHeight;

	private float _rewardWidth;

	private int _currentColumn;

	private float _startY;

	private float _startX;

	private int _bufferSize;

	private int _wantedSmallestRewardIndex;

	private int _loadedSmallestRewardIndex;

	private int _wantedLargestRewardIndex;

	private int _loadedLargestRewardIndex;

	private int Columns;

	private float _offscreenLoadTimer;

	private bool _scrollingRight;

	private int _velocityOffset;

	private int _lastRows;

	private int _lastRewardCount;

	private bool _autoScrolling;

	private int _autoScrollSpeed;

	private Action OnAutoScrollComplete;

	private Func<float> GetSecondsBetweenReveals;

	private bool _layoutUpdated;

	private ScrollRect.MovementType _defaultMovementType;

	private List<TransformAndIndex> _currentlyRevealedIndices;

	public bool ManualScrollDragAllowed
	{
		get
		{
			return _manualScrollDragAllowed;
		}
		set
		{
			_scrollRect.CanScroll = value;
			_scrollRect.CanDrag = value;
			_manualScrollDragAllowed = value;
		}
	}

	private RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = _scrollRect.GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public void Start()
	{
		_rewardTransforms = new List<Transform>();
		_currentlyRevealedIndices = new List<TransformAndIndex>();
		_defaultMovementType = _scrollRect.movementType;
		CalcRewardSize();
		_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
	}

	private void OnScrollRectValueChanged(Vector2 pos)
	{
		UpdateCurrentlyRevealedIndices();
	}

	public void SetTotalRewardCount(int count)
	{
		ClearAllTransforms();
		_totalRewards = count;
		_lastRewardCount = -1;
		_currentlyRevealedIndices = new List<TransformAndIndex>();
	}

	public void AutoScroll(Func<float> getSecondsBetweenReveals, Action onAutoScrollComplete = null)
	{
		OnAutoScrollComplete = (Action)Delegate.Combine(OnAutoScrollComplete, onAutoScrollComplete);
		GetSecondsBetweenReveals = getSecondsBetweenReveals;
		if (_scrollRect.content.rect.width > _scrollRect.viewport.rect.width)
		{
			_autoScrolling = true;
		}
		else
		{
			AutoScrollComplete();
		}
	}

	private float CalcAutoScrollSpeed()
	{
		return GetRewardPlusXSpacingWidth() / GetSecondsBetweenReveals() / (float)Rows;
	}

	public void StopScroll()
	{
		_scrollRect.StopMovement();
		AutoScrollComplete();
	}

	public void SnapToNormalizedX(float normalizedX)
	{
		ClearCurrentlyRevealedIndices();
		_scrollRect.normalizedPosition = new Vector2(normalizedX, 0f);
		_currentColumn = CalculateCurrentColumn(_scrollRect.content.anchoredPosition);
		if (_rewardTransforms.Count > 0)
		{
			UpdateRewards();
			UpdateOffscreenRewards();
			UpdateCurrentlyRevealedIndices();
		}
	}

	public List<TransformAndIndex> GetOnScreenTransforms()
	{
		return _currentlyRevealedIndices;
	}

	public float GetRewardPlusXSpacingWidth()
	{
		return _rewardWidth + _xSpaceBetweenRewards;
	}

	private void AutoScrollComplete()
	{
		_autoScrolling = false;
		if (OnAutoScrollComplete != null)
		{
			OnAutoScrollComplete();
			OnAutoScrollComplete = null;
		}
	}

	protected void Update()
	{
		if (_totalRewards != _lastRewardCount)
		{
			float x = _scrollRect.content.anchoredPosition.x;
			UpdateLayout();
			_scrollRect.content.anchoredPosition = new Vector2(x, _scrollRect.content.anchoredPosition.y);
			_lastRewardCount = _totalRewards;
		}
		if (_autoScrolling)
		{
			if ((_scrollRect.velocity.x > 0f || _scrollRect.velocity.y > 0f) && (_scrollRect.normalizedPosition.x < 0f || _scrollRect.normalizedPosition.x > 1f))
			{
				AutoScrollComplete();
			}
			else
			{
				_scrollRect.velocity = new Vector2(CalcAutoScrollSpeed(), 0f);
			}
		}
		if (_layoutUpdated)
		{
			UpdateCurrentlyRevealedIndices();
		}
	}

	private void UpdateCurrentlyRevealedIndices()
	{
		if (_rewardTransforms.Count == 0)
		{
			return;
		}
		for (int num = _currentlyRevealedIndices.Count - 1; num > -1; num--)
		{
			TransformAndIndex transformAndIndex = _currentlyRevealedIndices[num];
			if (!IndexOnScreen(transformAndIndex.index))
			{
				if (transformOffScreen != null)
				{
					transformOffScreen(transformAndIndex);
				}
				_currentlyRevealedIndices.Remove(transformAndIndex);
			}
		}
		int num2 = Math.Max(_loadedSmallestRewardIndex, 0);
		int num3 = Math.Min(_loadedLargestRewardIndex, _totalRewards - 1);
		if (_currentlyRevealedIndices.Count <= 0)
		{
			num2 = 0;
			num3 = _totalRewards - 1;
		}
		int i;
		for (i = num2; i <= num3; i++)
		{
			if (_currentlyRevealedIndices.FindIndex((TransformAndIndex m) => m.index == i) == -1 && IndexOnScreen(i))
			{
				TransformAndIndex transformAndIndex2 = new TransformAndIndex
				{
					index = i,
					transform = _rewardTransforms[i % _rewardTransforms.Count]
				};
				if (transformOnScreen != null)
				{
					transformOnScreen(transformAndIndex2);
				}
				_currentlyRevealedIndices.Add(transformAndIndex2);
			}
		}
	}

	private void ClearCurrentlyRevealedIndices()
	{
		foreach (TransformAndIndex currentlyRevealedIndex in _currentlyRevealedIndices)
		{
			if (transformOffScreen != null)
			{
				transformOffScreen(currentlyRevealedIndex);
			}
		}
		_currentlyRevealedIndices = new List<TransformAndIndex>();
	}

	private bool IndexOnScreen(int index)
	{
		Vector3 rewardPositionFromIndex = GetRewardPositionFromIndex(index);
		float num = _rewardWidth / 2f;
		float num2 = 0f - _scrollRect.content.anchoredPosition.x - num;
		float num3 = 0f - _scrollRect.content.anchoredPosition.x + _scrollRect.viewport.rect.width + num;
		if (rewardPositionFromIndex.x >= num2 && rewardPositionFromIndex.x <= num3)
		{
			return true;
		}
		return false;
	}

	private void LateUpdate()
	{
		if (_totalRewards <= 0)
		{
			return;
		}
		int num;
		if (!(_lastRect != RectTransform.rect))
		{
			num = ((_lastRows != Rows) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0041;
			}
		}
		else
		{
			num = 1;
		}
		UpdateLayout();
		goto IL_0041;
		IL_0041:
		ClampVelocity();
		if (!_rewardsDirty)
		{
			int num2 = CalculateCurrentColumn(_scrollRect.content.anchoredPosition);
			if (num2 != _currentColumn && num2 >= 0)
			{
				_scrollingRight = num2 > _currentColumn;
				_currentColumn = num2;
				_rewardsDirty = true;
			}
		}
		bool rewardsDirty = _rewardsDirty;
		if (_rewardsDirty)
		{
			UpdateRewards();
		}
		if (num == 0 && !rewardsDirty)
		{
			UpdateOffscreenRewards();
		}
		if (_layoutUpdated)
		{
			_layoutUpdated = false;
			if (layoutUpdateComplete != null)
			{
				layoutUpdateComplete();
			}
		}
	}

	private int CalculateCurrentColumn(Vector2 anchoredPosition)
	{
		int num = (int)Math.Floor((0f - anchoredPosition.x - _startX) / (_rewardWidth + _xSpaceBetweenRewards)) - 1;
		float value = Mathf.Floor((0f - _scrollRect.velocity.x) / (_rewardWidth + _xSpaceBetweenRewards));
		_velocityOffset = (int)Mathf.Clamp(value, -Columns / 2, Columns / 2);
		return num + _velocityOffset;
	}

	private void UpdateOffscreenRewards()
	{
		_offscreenLoadTimer += Time.deltaTime;
		if (!(_offscreenLoadTimer >= _offscreenLoadPulse))
		{
			return;
		}
		_offscreenLoadTimer = 0f;
		while (true)
		{
			if (_loadedLargestRewardIndex < _wantedLargestRewardIndex && (_scrollingRight || _loadedSmallestRewardIndex <= _wantedSmallestRewardIndex))
			{
				if (UpdateRewardView(_rewardTransforms, _loadedLargestRewardIndex++))
				{
					break;
				}
			}
			else if (_loadedSmallestRewardIndex < _wantedSmallestRewardIndex || UpdateRewardView(_rewardTransforms, _loadedSmallestRewardIndex--))
			{
				break;
			}
		}
	}

	private bool UpdateRewardView(List<Transform> rewardViews, int rewardIndex)
	{
		bool result = false;
		if (rewardViews.Count == 0)
		{
			return result;
		}
		Transform transform = rewardViews[rewardIndex % rewardViews.Count];
		if (rewardIndex >= 0 && rewardIndex < _totalRewards)
		{
			result = true;
			transform.gameObject.SetActive(value: true);
			transform.transform.localPosition = GetRewardPositionFromIndex(rewardIndex);
		}
		else
		{
			transform.gameObject.SetActive(value: false);
		}
		return result;
	}

	private void ClampVelocity()
	{
		if (_scrollRect.velocity.magnitude > _maxVelocity)
		{
			_scrollRect.velocity = _scrollRect.velocity.normalized * _maxVelocity;
		}
	}

	private float CalcRewardSize()
	{
		_lastRect = RectTransform.rect;
		_lastRows = Rows;
		_rewardHeight = (_lastRect.height - _ySpaceBetweenRewards * (float)(Rows - 1)) / (float)Rows;
		Rect rect = _mockReward.GetComponent<RectTransform>().rect;
		float num = _rewardHeight / rect.height;
		_rewardWidth = num * rect.width;
		return num;
	}

	private void UpdateLayout()
	{
		_rewardsDirty = true;
		float num = CalcRewardSize();
		float f = _lastRect.width / GetRewardPlusXSpacingWidth();
		Columns = Mathf.CeilToInt(f);
		if (Columns < 1)
		{
			Columns = 1;
		}
		int num2 = Rows * Columns;
		_startX = _rewardWidth * 0.5f + _paddingX;
		_currentColumn = 0;
		_loadedSmallestRewardIndex = 0;
		_loadedLargestRewardIndex = 0;
		_bufferSize = (int)Math.Floor(_bufferMultiplier * (float)num2);
		int num3 = num2 + _bufferSize * 2;
		_startY = (0f - _rewardHeight) / 2f;
		ResetWidth();
		if (_scrollRect.content.rect.width < _scrollRect.viewport.rect.width)
		{
			_startX += (_scrollRect.viewport.rect.width - _scrollRect.content.rect.width) / 2f;
			_scrollRect.movementType = ScrollRect.MovementType.Clamped;
		}
		else
		{
			_scrollRect.movementType = _defaultMovementType;
		}
		for (int i = _rewardTransforms.Count; i < num3; i++)
		{
			Transform item = GenerateTransform();
			_rewardTransforms.Add(item);
		}
		for (int num4 = _rewardTransforms.Count - 1; num4 > num3; num4--)
		{
			Transform obj = _rewardTransforms[num4];
			_rewardTransforms.RemoveAt(num4);
			UnityEngine.Object.Destroy(obj.gameObject);
		}
		for (int j = 0; j < _rewardTransforms.Count; j++)
		{
			Transform transform = _rewardTransforms[j];
			if (j < num3)
			{
				transform.transform.SetParent(_scrollRect.content);
				transform.transform.localScale = num * Vector3.one;
				transform.transform.localRotation = Quaternion.Euler(Vector3.zero);
				transform.transform.localPosition = GetRewardPositionFromIndex(j);
			}
			else
			{
				transform.transform.localScale = num * Vector3.one;
			}
			transform.gameObject.SetActive(value: false);
		}
		_layoutUpdated = true;
	}

	private void ResetWidth()
	{
		int num = Mathf.CeilToInt((float)_totalRewards / (float)Rows);
		_scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _paddingX * 2f + _rewardWidth * (float)num + (float)(num - 1) * _xSpaceBetweenRewards);
		_rewardsDirty = true;
	}

	private void UpdateRewards()
	{
		_rewardsDirty = false;
		int num = Rows * (Columns + 2 + Math.Abs(_velocityOffset));
		int num2 = Mathf.Max(_currentColumn - 1 - ((_velocityOffset > 0) ? _velocityOffset : 0), 0);
		for (int i = num2 * Rows; i < num2 * Rows + num; i++)
		{
			UpdateRewardView(_rewardTransforms, i);
		}
		UpdateBounds();
	}

	private void UpdateBounds()
	{
		int num = Rows * Columns;
		_wantedSmallestRewardIndex = Math.Max(_currentColumn * Rows - _bufferSize, 0);
		_wantedLargestRewardIndex = _currentColumn * Rows + (num + _bufferSize);
		_loadedSmallestRewardIndex = Math.Max(Math.Min(_loadedSmallestRewardIndex, _currentColumn * Rows), _wantedSmallestRewardIndex);
		_loadedLargestRewardIndex = Math.Min(Math.Max(_loadedLargestRewardIndex, _currentColumn * Rows + num - 1), _wantedLargestRewardIndex);
	}

	private Vector3 GetRewardPositionFromIndex(int index)
	{
		int num = index / Rows;
		int num2 = index % Rows;
		return new Vector3(_startX + (float)num * GetRewardPlusXSpacingWidth(), _startY + (float)num2 * (0f - (_rewardHeight + _ySpaceBetweenRewards)));
	}

	private Transform GenerateTransform()
	{
		GameObject obj = UnityEngine.Object.Instantiate(_mockReward, _scrollRect.content);
		obj.SetActive(value: true);
		return obj.transform;
	}

	public void ClearAllTransforms()
	{
		if (_rewardTransforms == null)
		{
			return;
		}
		foreach (Transform rewardTransform in _rewardTransforms)
		{
			foreach (Transform item in rewardTransform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		if (_scrollRect != null)
		{
			_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
		}
	}
}
