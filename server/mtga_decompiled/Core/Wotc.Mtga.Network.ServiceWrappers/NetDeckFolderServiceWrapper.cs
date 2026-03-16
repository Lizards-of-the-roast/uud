using System.Collections.Generic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.NetDeck;

namespace Wotc.Mtga.Network.ServiceWrappers;

public class NetDeckFolderServiceWrapper : INetDeckFolderServiceWrapper
{
	private readonly FrontDoorConnectionAWS _fdc;

	public static INetDeckFolderServiceWrapper Create()
	{
		return new NetDeckFolderServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}

	public NetDeckFolderServiceWrapper(FrontDoorConnectionAWS fdcAws)
	{
		_fdc = fdcAws;
	}

	public Promise<List<DTO_NetDeckFolder>> GetNetDeckFolders()
	{
		return _fdc.SendMessage<List<DTO_NetDeckFolder>>(CmdType.GetNetDeckFolders, (object)null);
	}
}
