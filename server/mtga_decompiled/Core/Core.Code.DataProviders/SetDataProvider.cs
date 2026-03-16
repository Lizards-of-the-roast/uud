using Core.Shared.Code.Network;

namespace Core.Code.DataProviders;

public class SetDataProvider
{
	private ISetDataServiceWrapper _serviceWrapper;

	public SetDataProvider(ISetDataServiceWrapper serviceWrapper)
	{
		_serviceWrapper = serviceWrapper;
	}
}
