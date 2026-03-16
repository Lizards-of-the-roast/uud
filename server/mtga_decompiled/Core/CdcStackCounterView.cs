using InteractionSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CdcStackCounterView : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshPro counterText;

	public uint parentInstanceId;

	private GameInteractionSystem _interactionSystem;

	public void Init(GameInteractionSystem interactionSystem, uint ParentInstanceId)
	{
		_interactionSystem = interactionSystem;
		parentInstanceId = ParentInstanceId;
	}

	public void SetCount(int count)
	{
		counterText.text = $"x{count}";
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		SimpleInteractionType interactionType = SimpleInteractionType.None;
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			interactionType = ((eventData.clickCount <= 1) ? SimpleInteractionType.Primary : SimpleInteractionType.DoublePrimary);
			break;
		case PointerEventData.InputButton.Right:
			interactionType = SimpleInteractionType.Secondary;
			break;
		}
		_interactionSystem?.OnStackClicked(this, interactionType);
		eventData.Use();
	}

	public void Cleanup()
	{
	}

	private void OnDestroy()
	{
		Cleanup();
	}
}
