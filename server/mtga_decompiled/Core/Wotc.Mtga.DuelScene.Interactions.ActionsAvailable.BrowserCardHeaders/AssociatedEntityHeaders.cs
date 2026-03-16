using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;

public class AssociatedEntityHeaders : ModalBrowserCardHeaderProvider.ISubProvider
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IEntityNameProvider<MtgEntity> _nameProvider;

	public AssociatedEntityHeaders(IGameStateProvider gameStateProvider, IEntityNameProvider<MtgEntity> nameProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_nameProvider = nameProvider ?? NullEntityNameProvider.Default;
	}

	public bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Action action, out ModalBrowserCardHeaderProvider.HeaderData headerData)
	{
		headerData = ModalBrowserCardHeaderProvider.HeaderData.Null;
		MtgEntity associatedEntity = GetAssociatedEntity(action, _gameStateProvider.CurrentGameState);
		if (associatedEntity == null)
		{
			return false;
		}
		string name = _nameProvider.GetName(associatedEntity);
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		headerData = new ModalBrowserCardHeaderProvider.HeaderData(useActionTypeHeader: true, name);
		return true;
	}

	private static MtgEntity GetAssociatedEntity(Action action, MtgGameState gameState)
	{
		uint alternativeGrpId = action.AlternativeGrpId;
		if (alternativeGrpId == 0)
		{
			return null;
		}
		uint instanceId = action.InstanceId;
		if (gameState.TryGetCard(instanceId, out var card))
		{
			for (int num = card.AbilityAdders.Count - 1; num >= 0; num--)
			{
				AddedAbilityData addedAbilityData = card.AbilityAdders[num];
				uint addedById = addedAbilityData.AddedById;
				if (addedAbilityData.AbilityId == alternativeGrpId && addedById != instanceId && gameState.TryGetEntity(addedById, out var mtgEntity))
				{
					return mtgEntity;
				}
			}
		}
		foreach (uint objectId in gameState.ObjectIds)
		{
			if (objectId == instanceId || !gameState.TryGetEntity(objectId, out var mtgEntity2))
			{
				continue;
			}
			foreach (AbilityPrintingData ability in mtgEntity2.Abilities)
			{
				if (ability.Id == alternativeGrpId)
				{
					return mtgEntity2;
				}
			}
		}
		return null;
	}
}
