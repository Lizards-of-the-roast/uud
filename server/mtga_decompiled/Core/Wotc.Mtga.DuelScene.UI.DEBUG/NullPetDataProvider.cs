using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullPetDataProvider : IPetDataProvider
{
	public static readonly IPetDataProvider Default = new NullPetDataProvider();

	public IReadOnlyList<(string petId, string variantId)> GetAllPetData()
	{
		return Array.Empty<(string, string)>();
	}
}
