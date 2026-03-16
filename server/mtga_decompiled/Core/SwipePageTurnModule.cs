using UnityEngine;
using UnityEngine.Events;
using Wotc.Mtga.CustomInput;

public class SwipePageTurnModule : MonoBehaviour
{
	private Vector3 _touchPositionFirst;

	private Vector3 _touchPositionLast;

	private float _minDragDistance;

	public UnityEvent onSwipeLeft;

	public UnityEvent onSwipeRight;

	private bool _dragging;

	private void Start()
	{
		_minDragDistance = Screen.height * 15 / 100;
	}

	private void Update()
	{
		if (_dragging || CustomInputModule.GetTouchCount() == 1 || Application.isEditor)
		{
			if (CustomInputModule.PointerWasPressedThisFrame())
			{
				_touchPositionFirst = CustomInputModule.GetPointerPosition();
				_dragging = true;
			}
			else if (CustomInputModule.PointerWasReleasedThisFrame())
			{
				_touchPositionLast = CustomInputModule.GetPointerPosition();
				_dragging = false;
				OnFinishDrag();
			}
		}
	}

	private void OnFinishDrag()
	{
		if ((Mathf.Abs(_touchPositionLast.x - _touchPositionFirst.x) > _minDragDistance || Mathf.Abs(_touchPositionLast.y - _touchPositionFirst.y) > _minDragDistance) && Mathf.Abs(_touchPositionLast.x - _touchPositionFirst.x) > Mathf.Abs(_touchPositionLast.y - _touchPositionFirst.y))
		{
			if (_touchPositionLast.x > _touchPositionFirst.x)
			{
				onSwipeRight.Invoke();
			}
			else
			{
				onSwipeLeft.Invoke();
			}
		}
	}
}
