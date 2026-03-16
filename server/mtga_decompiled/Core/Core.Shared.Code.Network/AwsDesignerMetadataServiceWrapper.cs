using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.Network;

public class AwsDesignerMetadataServiceWrapper : IDesignerMetadataServiceWrapper
{
	private FrontDoorConnectionAWS _frontDoorConnectionAws;

	public AwsDesignerMetadataServiceWrapper(FrontDoorConnectionAWS frontDoorConnectionAws)
	{
		_frontDoorConnectionAws = frontDoorConnectionAws;
	}

	public static IDesignerMetadataServiceWrapper Create()
	{
		return new AwsDesignerMetadataServiceWrapper(Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
	}

	public Promise<DTO_CardMetadataInfo> GetDesignerMetadata()
	{
		return _frontDoorConnectionAws.SendMessage<DTO_CardMetadataInfo>(CmdType.GetDesignerMetadata, (object)null);
	}
}
