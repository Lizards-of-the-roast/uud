using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface IRoundTripWorkflow
{
	bool CanHandleRequest(BaseUserRequest req);

	void OnRoundTrip(BaseUserRequest req);

	bool IsWaitingForRoundTrip();

	bool CanCleanupAfterOutboundMessage(ClientToGREMessage message);
}
