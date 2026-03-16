using InteractionSystem;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlefieldInput : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler
{
	[SerializeField]
	private View_Battlefield_ClickFeedback _feedbackPrefab;

	private GameManager _gameManagerCache;

	private GameInteractionSystem _gameInteractionSystem;

	private IUnityObjectPool _unityPool = NullUnityObjectPool.Default;

	public void SetInteractionSystem(GameInteractionSystem interactionSystem)
	{
		_gameInteractionSystem = interactionSystem;
	}

	public void SetUnityPool(IUnityObjectPool pool)
	{
		_unityPool = pool;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_gameInteractionSystem?.OnBattlefieldClicked();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		InstantiateFeedbackAtWorldPosition(eventData.pointerCurrentRaycast.worldPosition);
	}

	private void InstantiateFeedbackAtWorldPosition(Vector3 worldPosition)
	{
		View_Battlefield_ClickFeedback component = _unityPool.PopObject(_feedbackPrefab.gameObject).GetComponent<View_Battlefield_ClickFeedback>();
		component.SetObjectPool(_unityPool);
		component.transform.parent = base.transform;
		component.transform.position = worldPosition;
		component.BeginFeedback();
	}

	private void OnDestroy()
	{
		_gameInteractionSystem = null;
		_unityPool = NullUnityObjectPool.Default;
	}
}
