using UnityEngine;
using UnityEngine.EventSystems;

public class ExamineViewDismisser : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
{
	private GameManager _gameManagerCache;

	private GameManager GameManager
	{
		get
		{
			if (!_gameManagerCache)
			{
				_gameManagerCache = Object.FindObjectOfType<GameManager>();
			}
			return _gameManagerCache;
		}
	}

	public void Init(GameManager gameManager)
	{
		_gameManagerCache = gameManager;
	}

	private void DismissExamineView()
	{
		GameManager?.InteractionSystem?.FlagExamineViewForDismissal();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		DismissExamineView();
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		DismissExamineView();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		DismissExamineView();
	}

	public void OnScroll(PointerEventData eventData)
	{
	}
}
