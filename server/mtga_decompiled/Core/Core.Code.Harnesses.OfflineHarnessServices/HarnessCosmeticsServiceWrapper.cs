using System;
using System.Collections.Generic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessCosmeticsServiceWrapper : ICosmeticsServiceWrapper
{
	public Promise<CosmeticsClient> GetPlayerCosmetics()
	{
		throw new NotImplementedException();
	}

	public Promise<PreferredCosmetics> GetPlayerPreferredCosmetics()
	{
		throw new NotImplementedException();
	}

	public Promise<PreferredCosmetics> SetPetSelection(string name, string variant)
	{
		throw new NotImplementedException();
	}

	public Promise<PreferredCosmetics> SetAvatarSelection(string name)
	{
		throw new NotImplementedException();
	}

	public Promise<PreferredCosmetics> SetCardbackSelection(string name)
	{
		throw new NotImplementedException();
	}

	public Promise<PreferredCosmetics> SetEmotesSelection(List<string> emotes)
	{
		throw new NotImplementedException();
	}

	public Promise<PreferredCosmetics> SetTitleSelection(string titleId)
	{
		throw new NotImplementedException();
	}
}
