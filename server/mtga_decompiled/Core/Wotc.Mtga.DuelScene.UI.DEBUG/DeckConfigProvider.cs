using System.Collections.Generic;
using System.IO;
using GreClient.Network;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class DeckConfigProvider : IDeckConfigProvider
{
	private readonly string _directoryPath;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> _allDeckConfigs;

	public string RootDeckDirectory => _directoryPath;

	public IEnumerable<string> DecksByDirectory
	{
		get
		{
			IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> allDecks = GetAllDecks();
			foreach (var (subDirectory, readOnlyList2) in allDecks)
			{
				foreach (DeckConfig item in readOnlyList2)
				{
					yield return $"{subDirectory}{Path.DirectorySeparatorChar}{item}";
				}
			}
		}
	}

	public DeckConfigProvider(ICardDatabaseAdapter cardDatabase, string directoryPath)
	{
		_cardDatabase = cardDatabase;
		_directoryPath = directoryPath;
	}

	public IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> GetAllDecks()
	{
		return _allDeckConfigs ?? (_allDeckConfigs = LoadAllDeckConfigs());
	}

	public static DeckConfig ConstructDeckConfig(string deckName, DeckCollectionDeck deck)
	{
		return new DeckConfig(deckName, deck.mainDeckCards, deck.sideboardCards, deck.commanders, deck.companion);
	}

	private static IEnumerable<DeckConfig> ConvertToDeckConfigs(DeckCollection deckCollection)
	{
		foreach (string deckName in deckCollection.GetDeckNames())
		{
			DeckCollectionDeck deck = deckCollection.TryGetDeckByName(deckName);
			yield return ConstructDeckConfig(deckName, deck);
		}
	}

	private IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> LoadAllDeckConfigs()
	{
		Dictionary<string, IReadOnlyList<DeckConfig>> dictionary = new Dictionary<string, IReadOnlyList<DeckConfig>>();
		foreach (string item in LoadDirectories())
		{
			List<DeckConfig> list = new List<DeckConfig>(ConvertToDeckConfigs(new DeckCollection(_cardDatabase, GetDirectoryPath(item))));
			if (list.Count > 0)
			{
				dictionary[item] = list;
			}
		}
		return dictionary;
	}

	private IEnumerable<string> LoadDirectories()
	{
		if (Directory.Exists(_directoryPath))
		{
			yield return string.Empty;
			DirectoryInfo directoryInfo = new DirectoryInfo(_directoryPath);
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				yield return directoryInfo2.Name;
			}
		}
	}

	private string GetDirectoryPath(string subDirectory)
	{
		if (!string.IsNullOrEmpty(subDirectory))
		{
			return Path.Combine(_directoryPath, subDirectory);
		}
		return _directoryPath;
	}
}
