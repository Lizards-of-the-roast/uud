using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.NetDeck;

namespace Wotc.Mtga.Network.ServiceWrappers;

public interface INetDeckFolderServiceWrapper
{
	Promise<List<DTO_NetDeckFolder>> GetNetDeckFolders();
}
