using Wotc.Mtga.Cards.Database;

public class PromptTooltipData : TooltipData
{
	private readonly IPromptEngine _promptEngine;

	private readonly uint _promptId;

	public override string Text
	{
		get
		{
			if (_promptId != 0)
			{
				return _promptEngine.GetPromptText((int)_promptId);
			}
			return string.Empty;
		}
		set
		{
			_text = value;
		}
	}

	public PromptTooltipData(IPromptEngine promptEngine, uint promptId)
	{
		_promptEngine = promptEngine ?? NullPromptEngine.Default;
		_promptId = promptId;
	}
}
