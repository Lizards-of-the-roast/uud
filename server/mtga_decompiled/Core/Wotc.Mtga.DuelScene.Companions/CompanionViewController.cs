using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Companions;

public class CompanionViewController : ICompanionViewController
{
	private readonly ICompanionBuilder _builder;

	private readonly ICompanionDataProvider _dataProvider;

	private readonly ISignalDispatch<CompanionCreatedSignalArgs> _companionCreatedEvent;

	public CompanionViewController(ICompanionBuilder builder, ICompanionDataProvider dataProvider, ISignalDispatch<CompanionCreatedSignalArgs> companionCreatedEvent)
	{
		_builder = builder ?? NullCompanionBuilder.Default;
		_dataProvider = dataProvider ?? NullCompanionDataProvider.Default;
		_companionCreatedEvent = companionCreatedEvent;
	}

	public AccessoryController CreateCompanionForPlayer(MtgPlayer player)
	{
		uint instanceId = player.InstanceId;
		if (_dataProvider.TryGetCompanionDataForPlayer(instanceId, out var companionData))
		{
			AccessoryController accessoryController = _builder.Create(companionData);
			_companionCreatedEvent.Dispatch(new CompanionCreatedSignalArgs(this, instanceId, accessoryController));
			return accessoryController;
		}
		return null;
	}
}
