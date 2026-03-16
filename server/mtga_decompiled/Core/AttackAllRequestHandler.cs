using System.Collections.Generic;
using GreClient.Rules;
using Pooling;
using Wotc.Mtgo.Gre.External.Messaging;

public class AttackAllRequestHandler : BaseUserRequestHandler<DeclareAttackerRequest>
{
	private readonly IObjectPool _objectPool;

	public AttackAllRequestHandler(DeclareAttackerRequest request, IObjectPool objectPool)
		: base(request)
	{
		_objectPool = objectPool ?? new ObjectPool();
	}

	public override void HandleRequest()
	{
		List<Attacker> list = _objectPool.PopObject<List<Attacker>>();
		foreach (Attacker attacker in _request.Attackers)
		{
			if (attacker.SelectedDamageRecipient == null && attacker.LegalDamageRecipients.Count > 0)
			{
				attacker.SelectedDamageRecipient = attacker.LegalDamageRecipients[0];
				list.Add(attacker);
			}
		}
		_request.UpdateAttacker(list.ToArray());
		list.Clear();
		_objectPool.PushObject(list);
	}
}
