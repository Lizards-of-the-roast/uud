using System;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface IChooseXInterface
{
	event Action Submit;

	event Action<int> ValueModified;

	void Open();

	void Close();

	void SetButtonText(string text);

	void SetVisualState(NumericInputVisualState visualState);

	void SetButtonStyle(ButtonStyle.StyleType styleType);
}
