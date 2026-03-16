using System;

namespace Wotc.Mtga.DuelScene;

public class LanguageChangedMediator : IDisposable
{
	private readonly IPromptTextManager _promptTextManager;

	private readonly EntityViewManager _entityViewManager;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly ISignalListen _languageChangedSignal;

	public LanguageChangedMediator(IPromptTextManager promptTextManager, EntityViewManager entityViewManager, ICardHolderProvider cardHolderProvider, ISignalListen languageChangedSignal)
	{
		_promptTextManager = promptTextManager;
		_entityViewManager = entityViewManager;
		_cardHolderProvider = cardHolderProvider;
		_languageChangedSignal = languageChangedSignal;
		_languageChangedSignal.Listeners += OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		_promptTextManager.UpdateLanguage();
		_entityViewManager.UpdateLanguage();
		if (_cardHolderProvider.TryGetCardHolder(GREPlayerNum.LocalPlayer, CardHolderType.Hand, out HandCardHolder result))
		{
			result.OnLanguageChanged();
		}
	}

	public void Dispose()
	{
		_languageChangedSignal.Listeners -= OnLanguageChanged;
	}
}
