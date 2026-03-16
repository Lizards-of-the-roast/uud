using Core.Code.Collations;
using Core.Code.Collections;
using Core.Shared.Code.Network;
using Core.Shared.Code.Network.Utils;
using Cysharp.Threading.Tasks;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Sets;

namespace Core.Shared.Code.ServiceFactories;

public class LoadSetMetadataUniTask
{
	public async UniTask Load()
	{
		ISetMetadataProvider setMetaDataProvider = Pantry.Get<ISetMetadataProvider>();
		FormatManager formatManager = Pantry.Get<FormatManager>();
		FrontDoorResponseBridge<Wizards.Unification.Models.Sets.SetMetadataResponse, Wizards.Arena.Models.Network.SetMetadataResponse> collationMetadata = Pantry.Get<ISetMetadataServiceWrapper>().GetCollationMetadata();
		switch (collationMetadata.SerializationFormat)
		{
		case SerializationFormat.Json:
			await LoadSetMetadataJson(collationMetadata.JsonResponse, setMetaDataProvider, formatManager);
			break;
		case SerializationFormat.Protobuf:
			await LoadSetMetadataProtobuf(collationMetadata.BinaryResponse, setMetaDataProvider, formatManager);
			break;
		default:
			SimpleLog.LogError($"Invalid serialization format '{collationMetadata.SerializationFormat}'");
			break;
		}
	}

	private static async UniTask LoadSetMetadataJson(Promise<Wizards.Unification.Models.Sets.SetMetadataResponse> collationMetadataPromise, ISetMetadataProvider setMetaDataProvider, FormatManager formatManager)
	{
		await collationMetadataPromise.AsTask;
		if (collationMetadataPromise.Successful)
		{
			LoadSetMetadataFromCollection(SetServiceWrapperHelpers.ToSetMetadataCollection(collationMetadataPromise.Result), setMetaDataProvider, formatManager);
		}
	}

	private static async UniTask LoadSetMetadataProtobuf(Promise<Wizards.Arena.Models.Network.SetMetadataResponse> collationMetadataPromise, ISetMetadataProvider setMetaDataProvider, FormatManager formatManager)
	{
		await collationMetadataPromise.AsTask;
		if (collationMetadataPromise.Successful)
		{
			LoadSetMetadataFromCollection(SetServiceWrapperHelpers.ToSetMetadataCollection(collationMetadataPromise.Result), setMetaDataProvider, formatManager);
		}
	}

	private static void LoadSetMetadataFromCollection(SetMetadataCollection collection, ISetMetadataProvider setMetaDataProvider, FormatManager formatManager)
	{
		setMetaDataProvider.LoadData(collection);
		formatManager.SetupSetAvailabilities(setMetaDataProvider.GetSetAvailabilities());
	}
}
