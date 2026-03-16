using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class DesignerMetadataProvider : IDesignerMetadataProvider
{
	private readonly IDesignerMetadataServiceWrapper _designerMetadataServiceWrapper;

	private DTO_CardMetadataInfo _designerMetadata;

	private bool _initialized;

	public static DesignerMetadataProvider Create()
	{
		return new DesignerMetadataProvider(Pantry.Get<IDesignerMetadataServiceWrapper>());
	}

	public DesignerMetadataProvider(IDesignerMetadataServiceWrapper designerMetadataServiceWrapper)
	{
		_designerMetadataServiceWrapper = designerMetadataServiceWrapper;
	}

	public Promise<DTO_CardMetadataInfo> Initialize()
	{
		return _designerMetadataServiceWrapper.GetDesignerMetadata().Then(delegate(Promise<DTO_CardMetadataInfo> promise)
		{
			if (promise.Successful)
			{
				_designerMetadata = promise.Result ?? new DTO_CardMetadataInfo();
				_initialized = true;
			}
			else
			{
				PromiseExtensions.Logger.Error($"Failed to get designer metadata: {promise.Error}");
			}
		});
	}

	public DTO_CardMetadataInfo GetDesignerMetadata()
	{
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get designer metadata before data provider is initialized");
			return null;
		}
		return _designerMetadata;
	}
}
