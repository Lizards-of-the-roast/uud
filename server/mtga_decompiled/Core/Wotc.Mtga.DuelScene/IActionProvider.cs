using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface IActionProvider
{
	IReadOnlyList<ActionInfo> GetGameStateActions(uint instanceId);

	IReadOnlyList<GreInteraction> GetRequestActions(uint instanceId);
}
