using Wotc.Mtga.Loc;

namespace Core.Shared.Code.GreClientApi.Prompts;

public class UnityRiderPromptReplacer : IRiderPromptReplacer
{
	public string Replace(string input)
	{
		input = input.Replace("{0}", Languages.ActiveLocProvider.GetLocalizedText("Enum/SubType/SubType_SubType_None_0"));
		return input;
	}
}
