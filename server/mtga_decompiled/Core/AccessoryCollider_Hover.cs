using UnityEngine;

public class AccessoryCollider_Hover : MonoBehaviour
{
	public AccessoryController accessoryController;

	private void OnMouseOver()
	{
		accessoryController.HandleHoverEnter();
	}

	private void OnMouseExit()
	{
		accessoryController.HandleHoverExit();
	}
}
