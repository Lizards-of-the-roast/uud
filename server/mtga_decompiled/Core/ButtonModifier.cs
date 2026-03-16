using UnityEngine;

public abstract class ButtonModifier : MonoBehaviour
{
	public abstract void UpdateModifier(ButtonStateModifier.ButtonState buttonState);
}
