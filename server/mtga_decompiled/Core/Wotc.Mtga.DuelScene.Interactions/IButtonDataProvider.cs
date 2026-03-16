namespace Wotc.Mtga.DuelScene.Interactions;

public interface IButtonDataProvider
{
	string GetLocKey();

	string GetSfx();

	ButtonStyle.StyleType GetStyle();
}
