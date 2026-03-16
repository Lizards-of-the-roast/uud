using UnityEngine;
using UnityEngine.EventSystems;

public class NPEHangerClickDismisser : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private GameManager _gameManager;

	public void OnPointerClick(PointerEventData eventData)
	{
		_gameManager?.NpeDirector?.ClearNPEUXPrompts();
	}

	private void OnEnable()
	{
		_gameManager = Object.FindObjectOfType<GameManager>();
	}

	private void OnDisable()
	{
		_gameManager = null;
	}
}
