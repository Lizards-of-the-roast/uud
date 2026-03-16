namespace Wotc.Mtga.DuelScene.Interactions;

public interface IChooseXInterfaceBuilder
{
	IChooseXInterface CreateInterface(string assetTrackingKey);

	void DestroyInterface(IChooseXInterface chooseXInterface, string assetTrackingKey);
}
