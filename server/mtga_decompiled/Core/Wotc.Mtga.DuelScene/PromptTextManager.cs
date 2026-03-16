using System;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PromptTextManager : IPromptTextManager, IPromptTextProvider, IPromptTextController, IDisposable
{
	private readonly IPromptTextProvider _provider;

	private readonly IPromptTextController _controller;

	private Prompt _prompt;

	private string _key = string.Empty;

	private (string, string)[] _parameters;

	public PromptTextManager(IPromptTextProvider provider, IPromptTextController controller)
	{
		_provider = provider ?? NullPromptTextProvider.Default;
		_controller = controller ?? NullPromptTextManager.Default;
	}

	public string GetPromptText(Prompt prompt)
	{
		return _provider.GetPromptText(prompt);
	}

	public void SetPrompt(Prompt prompt)
	{
		_prompt = prompt;
		_key = string.Empty;
		_parameters = null;
		_controller.SetPrompt(prompt);
	}

	public void SetClientPrompt(string key, params (string, string)[] parameters)
	{
		_prompt = null;
		_key = key;
		_parameters = parameters;
		_controller.SetClientPrompt(key, parameters);
	}

	public void UpdateLanguage()
	{
		if (_prompt != null)
		{
			_controller.SetPrompt(_prompt);
		}
		else if (string.IsNullOrEmpty(_key))
		{
			_controller.SetClientPrompt(_key, _parameters);
		}
	}

	public void Dispose()
	{
		_prompt = null;
		_key = string.Empty;
		_parameters = null;
	}
}
