using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Loc.CachingPatterns;

public abstract class BotBattleConfigView : MonoBehaviour
{
	private CardDatabase _localCardDatabase;

	public abstract BotBattleSessionType PanelType { get; protected set; }

	public abstract BotBattleDSConfig GetConfig();

	protected CardDatabase GetLocalCardDatabase()
	{
		if (_localCardDatabase == null)
		{
			IBILogger biLogger = new BILogger();
			string currentDataSource = DataSourceUtilities.GetCurrentDataSource();
			ISqlHelper sqlHelper = Pantry.Get<ISqlHelper>();
			_localCardDatabase = CardDatabase.CreateCardDatabase(AssetLoader.GetRawFilePath(currentDataSource, "CardDatabase.sqlite"), AssetLoader.GetRawFilePath(currentDataSource, "altFlavorTexts.json"), AssetLoader.GetRawFilePath(currentDataSource, "altArtCredits.json"), new SqlLocalizationManager(AssetLoader.GetRawFilePath(ClientPathUtilities.DefaultLocFolder, "ClientLocalization.sqlite"), biLogger, new DictionaryCache<string, string>(1000), sqlHelper), biLogger, sqlHelper, Pantry.Get<IAccountClient>(), useAndroidFix: false);
		}
		return _localCardDatabase;
	}
}
