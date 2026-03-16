using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Shared.Code.AutoPlay.TimedReplays;
using Google.Protobuf;
using GreClient.Network;
using Newtonsoft.Json;
using UnityEngine;
using Wizards.Mtga.IO;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.TimedReplays;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Replays;

public static class ReplayUtilities
{
	private static readonly KnownTypesBinder knownTypesBinder = new KnownTypesBinder
	{
		KnownTypes = new List<Type>
		{
			typeof(IMessage),
			typeof(ConnectResp)
		}
	};

	private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
	{
		Converters = new List<JsonConverter>
		{
			new ProtoMessageConverter()
		},
		SerializationBinder = knownTypesBinder
	};

	public static void SaveReplay(string name, string rootPath, IReadOnlyList<IMessage> messages, bool openInExplorer = false)
	{
		try
		{
			if (messages.Count == 0)
			{
				Debug.LogError("[DebugConsoleBehavior] No messages to save");
				return;
			}
			string text = Path.Combine(rootPath, name + ".replay");
			if (File.Exists(text))
			{
				Debug.LogError("[DebugConsoleBehavior] Session \"" + name + "\" already exists. Pick a different name.");
				return;
			}
			if (!Directory.Exists(rootPath))
			{
				Directory.CreateDirectory(rootPath);
			}
			File.WriteAllLines(text, messages.ToList().ConvertAll<string>(SerializeProtoMessage).ToArray());
			Debug.Log("Session recording saved to " + text);
			if (openInExplorer)
			{
				Application.OpenURL(Path.GetDirectoryName(text));
			}
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("[DebugConsoleBehavior] Exception saving session: {0}", ex);
		}
	}

	public static string GetReplayFolder()
	{
		if (!Application.isEditor && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
		{
			return Path.Combine(Application.persistentDataPath, "Replays");
		}
		return Path.Combine(Application.streamingAssetsPath, "Tests");
	}

	public static string GetNewTimedReplayPath()
	{
		string replayFolder = GetReplayFolder();
		if (!WindowsSafePath.DirectoryExists(replayFolder))
		{
			WindowsSafePath.CreateDirectory(replayFolder);
		}
		string text = MDNPlayerPrefs.ReplayName;
		if (!string.IsNullOrEmpty(text))
		{
			text += "_";
		}
		int num = 0;
		string text2;
		do
		{
			text2 = Path.Combine(replayFolder, $"{text}Replay{num}.rply");
			num++;
		}
		while (WindowsSafePath.FileExists(text2));
		return text2;
	}

	public static void StartReplay(ReplayInfo replayInfo, PAPA papa, MatchManager matchManager, ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, System.Action onLoad, string sceneToLoad = null)
	{
		IGREConnection iGREConnection;
		if (replayInfo.Format == ReplayFormat.TimedReplay)
		{
			TimedReplayPlayer.NextReplay = replayInfo.ReplayPath;
			iGREConnection = new TimedReplayGREmlin(replayInfo.ReplayPath);
		}
		else
		{
			iGREConnection = new Gremlin(replayInfo.ReplayPath, replayInfo.Format);
		}
		matchManager.Initialize(iGREConnection, cardDatabase, GameSessionType.Replay);
		if (iGREConnection is TimedReplayGREmlin timedReplayGREmlin)
		{
			timedReplayGREmlin.SetUpCosmetics(matchManager, papa.AssetLookupSystem);
		}
		MatchSceneManager.Load(papa, sceneToLoad, cardDatabase, cardViewBuilder, cardMaterialBuilder, onMatchSceneLoaded);
		void onConnectedToMatchService()
		{
			matchManager.OnConnectedToService -= onConnectedToMatchService;
			matchManager.JoinMatch("REPLAY_URI", "REPLAY_MATCH_ID");
		}
		void onMatchSceneLoaded()
		{
			matchManager.OnConnectedToService += onConnectedToMatchService;
			matchManager.ConnectToMatchService(null);
			onLoad?.Invoke();
		}
	}

	public static List<ReplayInfo> FindReplayInfo(string replayFolder = null, ReplayFormat[] formats = null)
	{
		List<ReplayInfo> list = new List<ReplayInfo>();
		Dictionary<ReplayFormat, List<string>> dictionary = FindReplays(replayFolder, formats);
		foreach (ReplayFormat key in dictionary.Keys)
		{
			foreach (string item in dictionary[key])
			{
				list.Add(new ReplayInfo(item, key));
			}
		}
		return list;
	}

	public static Dictionary<ReplayFormat, List<string>> FindReplays(string rootFolder = null, ReplayFormat[] types = null)
	{
		if (types == null)
		{
			types = new ReplayFormat[3]
			{
				ReplayFormat.Compressed,
				ReplayFormat.Text,
				ReplayFormat.JsonFilesInFolder
			};
		}
		if (rootFolder == null)
		{
			rootFolder = GetReplayFolder();
		}
		Dictionary<ReplayFormat, List<string>> dictionary = new Dictionary<ReplayFormat, List<string>>();
		if (!WindowsSafePath.DirectoryExists(rootFolder))
		{
			return dictionary;
		}
		DirectoryInfo directoryInfo = WindowsSafePath.CreateDirectory(rootFolder);
		if (types.Contains(ReplayFormat.JsonFilesInFolder))
		{
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			dictionary.Add(ReplayFormat.JsonFilesInFolder, directories.Select((DirectoryInfo file) => file.FullName).ToList());
		}
		if (types.Contains(ReplayFormat.Text))
		{
			List<string> value = (from f in directoryInfo.GetFiles("*.replay")
				select f.FullName).ToList();
			dictionary.Add(ReplayFormat.Text, value);
		}
		if (types.Contains(ReplayFormat.Compressed))
		{
			List<string> value2 = (from f in directoryInfo.GetFiles("*.replay.gz")
				select f.FullName).ToList();
			dictionary.Add(ReplayFormat.Compressed, value2);
		}
		if (types.Contains(ReplayFormat.TimedReplay))
		{
			List<string> value3 = (from f in directoryInfo.GetFiles("*.rply")
				select f.FullName).ToList();
			dictionary.Add(ReplayFormat.TimedReplay, value3);
		}
		return dictionary;
	}

	public static string SerializeProtoMessage(IMessage message)
	{
		return JsonConvert.SerializeObject(message, _jsonSerializerSettings);
	}

	public static IMessage DeserializeAutoPlayJSON<T>(string rawMessage) where T : IMessage
	{
		return JsonConvert.DeserializeObject<T>(rawMessage, _jsonSerializerSettings);
	}

	public static List<IMessage> LoadRecordedMessages(string path, ReplayFormat replayFormat)
	{
		try
		{
			List<string> list = new List<string>();
			switch (replayFormat)
			{
			case ReplayFormat.JsonFilesInFolder:
			{
				if (!Directory.Exists(path))
				{
					SimpleLog.LogErrorFormat("Path \"{0}\" passed in as type {1}.", path, replayFormat);
					break;
				}
				List<string> list2 = new List<string>(Directory.GetFiles(path, "*.json"));
				list2.Sort((string x, string y) => int.Parse(Path.GetFileNameWithoutExtension(x)).CompareTo(int.Parse(Path.GetFileNameWithoutExtension(y))));
				list.AddRange(list2.ConvertAll((string x) => File.ReadAllText(x)));
				break;
			}
			case ReplayFormat.Text:
				if (!path.EndsWith(".replay"))
				{
					SimpleLog.LogErrorFormat("Path \"{0}\" passed in as type {1}.", path, replayFormat);
				}
				else if (File.Exists(path))
				{
					list.AddRange(File.ReadAllLines(path));
				}
				break;
			case ReplayFormat.Compressed:
				if (!path.EndsWith(".replay.gz"))
				{
					SimpleLog.LogErrorFormat("Path \"{0}\" passed in as type {1}.", path, replayFormat);
				}
				else if (File.Exists(path))
				{
					byte[] bytes = CompressionUtilities.DecompressBytes(File.ReadAllBytes(path));
					string text = Encoding.UTF8.GetString(bytes);
					list.AddRange(text.Split(new string[1] { "/r/n" }, StringSplitOptions.RemoveEmptyEntries));
				}
				break;
			case ReplayFormat.TimedReplay:
				throw new Exception("Timed replays are not a supported replay type for GRE tests");
			}
			List<IMessage> list3 = new List<IMessage>();
			foreach (string item in list)
			{
				if (item.Contains("GREMessageType"))
				{
					list3.Add(DeserializeAutoPlayJSON<GREToClientMessage>(item));
				}
				else if (item.Contains("ClientMessageType"))
				{
					list3.Add(DeserializeAutoPlayJSON<ClientToGREMessage>(item));
				}
			}
			return list3;
		}
		catch (Exception e)
		{
			SimpleLog.LogException(e);
			return new List<IMessage>();
		}
	}
}
