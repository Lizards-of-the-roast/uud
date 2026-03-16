using System;
using AssetLookupTree.Blackboard;
using Core.Code.AssetLookupTree.AssetLookup;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Shared.Code.ServiceFactories;

public class LoadCardDatabaseUniTask
{
	public static CardDatabaseLoader CreateLoader()
	{
		string currentDataSource = DataSourceUtilities.GetCurrentDataSource();
		return new CardDatabaseLoader(AssetLoader.GetRawFilePath(currentDataSource, "CardDatabase.sqlite"), AssetLoader.GetRawFilePath(currentDataSource, "altFlavorTexts.json"), AssetLoader.GetRawFilePath(currentDataSource, "altArtCredits.json"), Pantry.Get<IClientLocProvider>(), Pantry.Get<IBILogger>(), Pantry.Get<ISqlHelper>(), Pantry.Get<IAccountClient>(), useAndroidFix: false);
	}

	public async UniTask Load()
	{
		IBILogger biLogger = Pantry.Get<IBILogger>();
		CardDatabaseLoader cardDatabaseLoader = Pantry.Get<CardDatabaseLoader>();
		cardDatabaseLoader.StartLoad();
		await UniTask.WaitUntil(() => cardDatabaseLoader.IsComplete && !cardDatabaseLoader.Cancelled);
		if (cardDatabaseLoader.Exception != null)
		{
			ResourceError resourceError = new ResourceError
			{
				Message = "Failed to load card database",
				Error = cardDatabaseLoader.Exception.Message,
				EventTime = DateTime.UtcNow
			};
			Debug.LogException(cardDatabaseLoader.Exception);
			biLogger.Send(ClientBusinessEventType.ResourceError, resourceError);
			Pantry.Get<ResourceErrorMessageManager>().ShowError(resourceError.Message, resourceError.Error);
		}
		else
		{
			CardDatabase cardDatabase = cardDatabaseLoader.CardDatabase;
			(Pantry.Get<AssetLookupManager>().AssetLookupSystem.Blackboard as Blackboard)?.Inject(cardDatabase);
			BaseSortedCardCache.ClearAllCachedLists();
		}
	}
}
