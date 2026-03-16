using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public interface IEventTranslation
{
	List<UXEvent> GenerateEvents(GameStateUpdate gameStateUpdate);
}
