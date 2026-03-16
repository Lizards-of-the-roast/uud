public interface ICustomButtonAnimationHandler
{
	void BeginDisabled();

	void BeginDisabled(float duration);

	void BeginMouseOff();

	void BeginMouseOff(float duration);

	void BeginMouseOver();

	void BeginMouseOver(float duration);

	void BeginPressedOver();

	void BeginPressedOver(float duration);

	void BeginPressedOff();

	void BeginPressedOff(float duration);
}
