namespace Wizards.Mtga.PlayBlade;

public interface IPlayBladeSelectionProvider
{
	BladeSelectionData GetSelection();

	void SetSelection(BladeSelectionData data);

	void SetSelectedTab(BladeType bladeType);

	bool IsEventBladeDeckSelected();

	BladeSelectionData GetDefaultSelectionData();
}
