using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Extensions;

public static class DeclareAttackersReqExtensions
{
	public static IReadOnlyList<ManaRequirement> GetNonEmptyManaCosts(this DeclareAttackersReq req)
	{
		if (req != null)
		{
			return req.ManaCost.GetNonEmptyManaCosts();
		}
		return Array.Empty<ManaRequirement>();
	}
}
