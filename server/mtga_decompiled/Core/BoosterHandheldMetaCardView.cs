using UnityEngine.EventSystems;

public class BoosterHandheldMetaCardView : BoosterMetaCardView
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		base.OnPointerExit(eventData);
	}
}
