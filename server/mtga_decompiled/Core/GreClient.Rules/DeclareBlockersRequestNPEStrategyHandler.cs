using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Rules;

public class DeclareBlockersRequestNPEStrategyHandler : BaseUserRequestHandler<DeclareBlockersRequest>
{
	private List<BlockToMake> _blocksToMake;

	private Blocker _blocker;

	public DeclareBlockersRequestNPEStrategyHandler(DeclareBlockersRequest request, List<BlockToMake> blocksToMake, Blocker blocker)
		: base(request)
	{
		_blocksToMake = blocksToMake;
		_blocker = blocker;
	}

	public override void HandleRequest()
	{
		if (_blocksToMake.Count == 0)
		{
			_request.SubmitBlockers();
		}
		else if (_blocker != null)
		{
			_blocker.SelectedAttackerInstanceIds.Add(_blocksToMake.First().AttackerId);
			_request.UpdateBlockers(_blocker);
		}
	}
}
