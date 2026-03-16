using UnityEngine;

[CreateAssetMenu(fileName = "TooltipPropertiesObject", menuName = "ScriptableObject/TooltipPropertiesObject", order = 0)]
public class TooltipPropertiesObject : ScriptableObject
{
	public TooltipProperties Properties = new TooltipProperties();
}
