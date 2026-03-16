namespace Wotc.Mtga.DuelScene;

public interface IPromptTextManager : IPromptTextProvider, IPromptTextController
{
	void UpdateLanguage();
}
