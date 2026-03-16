using UnityEngine;

[CreateAssetMenu(fileName = "AE_SM_New", menuName = "ScriptableObject/AccessoryEvents/SwapMaterial", order = 1)]
public class AccessoryEvent_SwapMaterial : AccessoryEventSO
{
	[SerializeField]
	private Material newMaterial;

	[SerializeField]
	private int materialSlotIndex;

	public void Execute(Renderer renderer)
	{
		Material[] materials = renderer.materials;
		materials[materialSlotIndex] = newMaterial;
		renderer.materials = materials;
	}
}
