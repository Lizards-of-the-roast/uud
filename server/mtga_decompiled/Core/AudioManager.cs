using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.General;
using AssetLookupTree.Payloads.Resolution;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using UnityEngine;
using Wizards.Mtga.Storage;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class AudioManager : MonoBehaviour
{
	private const string StartupBankName = "STARTUP";

	private const string defaultAmbEventName = "sfx_amb_mainnav_start";

	private static List<PendingEvent> _events = new List<PendingEvent>();

	public bool _musicIsPlaying;

	public bool AmbIsPlaying;

	private bool initialized;

	private List<string> loadedPcks = new List<string>();

	private List<string> loadedBnks = new List<string>();

	private string loadedBatPck = string.Empty;

	private string loadedBatBnk = string.Empty;

	private static float internalvolume = 100f;

	private static string ambkey = string.Empty;

	public bool interacted;

	private static KeyValuePair<string, bool> ambiance;

	private AudioHistory eventlog = new AudioHistory();

	private static AudioManager _instance;

	private static GameObject _default;

	protected AssetLookupSystem _assetLookupSystem;

	protected AssetLookupTree<Music> _musicTree;

	protected AssetLookupTree<Ambiance> _ambianceTree;

	private static IStorageContext _storageContext;

	private bool EnableWwiseStayAwake;

	private static Regex BankNameExtractorRegex = new Regex("^(.+?)(?:_[a-f0-9]{8,128})?\\.[^.]+$");

	public bool Initialized => initialized;

	public AudioHistory EventHistory => eventlog;

	public static AudioManager Instance => _instance;

	public static GameObject Default
	{
		get
		{
			if (!(_default != null))
			{
				return _default = _instance.transform.GetChild(0).gameObject;
			}
			return _default;
		}
	}

	private static IEnumerable<string> AvailableAudioPackages => from p in AssetLoader.GetAudioPackagePaths()
		select p.Substring(p.LastIndexOf(Path.DirectorySeparatorChar) + 1);

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	private void OnApplicationQuit()
	{
		PlayerPrefsExt.Save();
		UnInitializeAudio();
		UnityEngine.Object.Destroy(_instance.gameObject);
	}

	private void OnApplicationFocus(bool focus)
	{
		EnableWwiseStayAwake = !focus;
		if (!MDNPlayerPrefs.PLAYERPREFS_KEY_BACKGROUNDAUDIO)
		{
			SetRTPCValue("MasterVolume", focus ? GetMasterVolume() : 0f);
		}
	}

	private void WwiseStayAwake()
	{
		if (EnableWwiseStayAwake)
		{
			AkSoundEngine.WakeupFromSuspend();
			AkSoundEngine.RenderAudio();
		}
	}

	private void Update()
	{
		WwiseStayAwake();
		if (_events.Count == 0)
		{
			return;
		}
		int num = 0;
		while (num < _events.Count)
		{
			_events[num]._delay -= Time.deltaTime;
			if (_events[num]._delay <= 0f)
			{
				if (_events[num].isStop)
				{
					ExecuteActionOnEvent(_events[num]._eventname, AkActionOnEventType.AkActionOnEventType_Stop, _events[num]._object);
				}
				else
				{
					PostEvent(_events[num]._eventname, _events[num]._object);
				}
				_events.Remove(_events[num]);
			}
			else
			{
				num++;
			}
		}
	}

	public void OnPriorityUpdated(GREPlayerNum obj)
	{
		float value = ((obj == GREPlayerNum.LocalPlayer) ? 0f : 100f);
		SetRTPCValue("Intensity", value, Default, 1, AkCurveInterpolation.AkCurveInterpolation_InvSCurve);
	}

	public static void PlayMusic(string sceneToLoad = null)
	{
		if (!_instance._musicIsPlaying)
		{
			PostEvent("Music_Play", Default);
			_instance._musicIsPlaying = true;
		}
		Instance._assetLookupSystem.Blackboard.Clear();
		Instance._assetLookupSystem.Blackboard.BattlefieldId = BattlefieldUtil.BattlefieldId;
		Instance._assetLookupSystem.Blackboard.InDuelScene = sceneToLoad == "DuelScene";
		if (!Enum.TryParse<NavContentType>(sceneToLoad, out var result))
		{
			result = NavContentType.None;
		}
		Instance._assetLookupSystem.Blackboard.NavContentType = result;
		Music music = null;
		if (Instance._musicTree == null)
		{
			Instance._musicTree = Instance._assetLookupSystem.TreeLoader.LoadTree<Music>(returnNewTree: false);
		}
		if (Instance._musicTree != null)
		{
			music = Instance._musicTree.GetPayload(Instance._assetLookupSystem.Blackboard);
		}
		if (music != null)
		{
			if (music.IsState)
			{
				SetState("music", music.MusicEvent);
			}
			else
			{
				PostEvent(music.MusicEvent, Default);
			}
		}
	}

	public static void StopMusic()
	{
		if (_instance._musicIsPlaying)
		{
			PostEvent("Music_Stop", Default);
			_instance._musicIsPlaying = false;
		}
	}

	public static void StopSFX(GameObject obj)
	{
		if (obj != null)
		{
			AkSoundEngine.StopAll(obj);
		}
	}

	public static void PlayAmbiance(string sceneToLoad = null)
	{
		if (Instance.AmbIsPlaying)
		{
			StopAmbiance();
		}
		if (string.IsNullOrEmpty(sceneToLoad))
		{
			PostEvent("sfx_amb_mainnav_start", Default);
			Instance.AmbIsPlaying = true;
			return;
		}
		Instance._assetLookupSystem.Blackboard.Clear();
		Instance._assetLookupSystem.Blackboard.BattlefieldId = BattlefieldUtil.BattlefieldId;
		Instance._assetLookupSystem.Blackboard.InDuelScene = sceneToLoad == "DuelScene";
		if (!Enum.TryParse<NavContentType>(sceneToLoad, out var result))
		{
			result = NavContentType.None;
		}
		Instance._assetLookupSystem.Blackboard.NavContentType = result;
		Ambiance ambiance = null;
		if (Instance._ambianceTree == null)
		{
			Instance._ambianceTree = Instance._assetLookupSystem.TreeLoader.LoadTree<Ambiance>(returnNewTree: false);
		}
		if (Instance._ambianceTree != null)
		{
			ambiance = Instance._ambianceTree.GetPayload(Instance._assetLookupSystem.Blackboard);
		}
		if (ambiance != null)
		{
			if (ambiance.IsState)
			{
				SetState("nav_amb", ambiance.StartEvent);
			}
			else
			{
				PostEvent(ambiance.StartEvent, Default);
			}
			AudioManager.ambiance = new KeyValuePair<string, bool>(ambiance.StopEvent, ambiance.IsState);
			Instance.AmbIsPlaying = true;
		}
	}

	public static void StopAmbiance()
	{
		if (!ambiance.Equals(default(KeyValuePair<string, bool>)))
		{
			if (ambiance.Value)
			{
				SetState("nav_amb", "None");
			}
			else
			{
				PostEvent(ambiance.Key, Default);
			}
		}
		Instance.AmbIsPlaying = false;
	}

	private static void AddPendingEvent(PendingEvent vent)
	{
		int num = -1;
		if (vent._eventname.Contains("Music_SetState"))
		{
			num = _events.FindIndex((PendingEvent x) => x._eventname == "Music_SetState_Battlefield_GRN_baselayer");
			if (num != -1)
			{
				_events[num]._delay = 75f;
			}
		}
		if (vent._eventname != "Music_SetState_Battlefield_GRN_baselayer" || num == -1)
		{
			_events.Add(vent);
		}
	}

	public string[] BuildETBAudioEvents(ICardDataAdapter card)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < card.Subtypes.Count; i++)
		{
			string text = card.Subtypes[i].ToString();
			string text2 = (string.IsNullOrEmpty(text) ? "_nul" : text);
			list.Add("sfx_c_type_etb_" + text2);
		}
		return list.ToArray();
	}

	private string[] BuildETBAudioEvents(BASE_CDC card)
	{
		return BuildETBAudioEvents(card.Model);
	}

	public void PlayAudio_BoosterETB(BASE_CDC card, float delay = 0f)
	{
		IReadOnlyList<CardType> cardTypes = card.Model.CardTypes;
		List<AudioEvent> list = new List<AudioEvent>();
		HashSet<EtbSFX> hashSet = new HashSet<EtbSFX>();
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(card.Model);
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EtbSFX> loadedTree) && loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet))
		{
			foreach (EtbSFX item in hashSet)
			{
				list.AddRange(item.SfxData.AudioEvents);
			}
		}
		_assetLookupSystem.Blackboard.Clear();
		End_SFX end_SFX = null;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<End_SFX> loadedTree2))
		{
			end_SFX = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
		}
		if (cardTypes.Contains(CardType.Land) || cardTypes.Contains(CardType.Artifact))
		{
			PlayAudio_ETB(card, delay);
			return;
		}
		if (cardTypes.Contains(CardType.Planeswalker) && list.Count > 0)
		{
			foreach (AudioEvent item2 in list)
			{
				if (!item2.WwiseEventName.Contains("walker"))
				{
					PlayAudio(item2.WwiseEventName, card.Root.gameObject, delay);
				}
			}
			return;
		}
		if (cardTypes.Contains(CardType.Creature))
		{
			if (list.Count > 0)
			{
				foreach (AudioEvent item3 in list)
				{
					if (!item3.WwiseEventName.Contains("loop"))
					{
						if (item3.WwiseEventName.Contains("buff"))
						{
							PlayAudio_ETB(card, delay);
							break;
						}
						PlayAudio(item3.WwiseEventName, card.Root.gameObject, item3.Delay);
					}
				}
				return;
			}
			PlayAudio_ETB(card, delay);
			return;
		}
		if (cardTypes.Contains(CardType.Instant) && end_SFX != null)
		{
			foreach (AudioEvent audioEvent in end_SFX.SfxData.AudioEvents)
			{
				PlayAudio(audioEvent.WwiseEventName, card.Root.gameObject, audioEvent.Delay);
			}
			return;
		}
		if (card.name.Contains("Wildcard"))
		{
			PlayAudio("sfx_ui_main_cardpack_wild_flip", card.Root.gameObject, delay);
		}
		else
		{
			PlayAudio("sfx_ui_small_magical_swish", card.Root.gameObject, delay);
		}
	}

	public void PlayAudio_ETB(BASE_CDC card, float delay = 0f)
	{
		if (card.Model.CardTypes.Contains(CardType.Creature))
		{
			SetSwitch("color", GetColorKey(card.Model.PresentationColor), card.gameObject);
			string[] array = BuildETBAudioEvents(card);
			int num = ((array.Length < 3) ? array.Length : 3);
			for (int i = 0; i < num; i++)
			{
				AddPendingEvent(new PendingEvent(0.5f * (float)i + delay, array[i], card.Root.gameObject));
			}
		}
	}

	public static void PlayAudio(AudioEvent audioEvent, GameObject target)
	{
		PlayAudio(audioEvent.WwiseEventName, audioEvent.PlayOnGlobal ? Default : target, audioEvent.Delay);
	}

	public static void PlayAudio(List<AudioEvent> audioEvents, GameObject target)
	{
		foreach (AudioEvent audioEvent in audioEvents)
		{
			PlayAudio(audioEvent.WwiseEventName, audioEvent.PlayOnGlobal ? Default : target, audioEvent.Delay);
		}
	}

	public static void PlayAudio(List<AudioEvent> audioEvents, GameObject target, float delay = 0f)
	{
		foreach (AudioEvent audioEvent in audioEvents)
		{
			PlayAudio(audioEvent.WwiseEventName, audioEvent.PlayOnGlobal ? Default : target, audioEvent.Delay + delay);
		}
	}

	public static void PlayAudio(string eventId, GameObject target)
	{
		PostEvent(eventId, target);
	}

	public static void PlayAudio(WwiseEvents eventKey, GameObject target)
	{
		PostEvent(eventKey.EventName, target);
	}

	public static void SetSwitch(string switchGroup, string switchState, GameObject obj)
	{
		if (AkSoundEngine.SetSwitch(switchGroup, switchState, obj) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to set {0} in group {1} on object: {2}", switchState, switchGroup, obj.name);
		}
	}

	public static void SetState(string stateGroup, string state)
	{
		if (AkSoundEngine.SetState(stateGroup, state) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to set {0} in group {1}", state.ToString(), stateGroup.ToString());
		}
	}

	public static void PostStopEvent(string eventName, GameObject obj, float delay = 0f)
	{
		uint[] out_aPlayingIDs = new uint[0];
		uint io_ruNumIDs = 0u;
		if (AkSoundEngine.GetPlayingIDsFromGameObject(obj, ref io_ruNumIDs, out_aPlayingIDs) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to get playingIds from  {0} ", (obj != null) ? obj.name : "Null game object");
		}
		if (io_ruNumIDs != 0)
		{
			if (delay == 0f)
			{
				AkSoundEngine.StopAll(obj);
				return;
			}
			_events.Add(new PendingEvent(delay, eventName, obj)
			{
				isStop = true
			});
		}
	}

	public static void ExecuteActionOnEvent(string akevent, AkActionOnEventType action, GameObject obj)
	{
		if (!string.IsNullOrEmpty(akevent))
		{
			AkSoundEngine.ExecuteActionOnEvent(akevent, action, obj);
		}
	}

	public static bool IsSoundsPlayingOnObject(GameObject obj)
	{
		uint[] out_aPlayingIDs = new uint[0];
		uint io_ruNumIDs = 0u;
		if (AkSoundEngine.GetPlayingIDsFromGameObject(obj, ref io_ruNumIDs, out_aPlayingIDs) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to get playingIds from  {0} ", (obj != null) ? obj.name : "Null game object");
		}
		return io_ruNumIDs != 0;
	}

	public static List<uint> GetPlayingIds(GameObject obj)
	{
		uint[] array = new uint[0];
		uint io_ruNumIDs = 0u;
		if (AkSoundEngine.GetPlayingIDsFromGameObject(obj, ref io_ruNumIDs, array) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to get playingIds from  {0} ", (obj != null) ? obj.name : "Null game object");
		}
		if (io_ruNumIDs != 0)
		{
			return array.ToList();
		}
		return new List<uint>();
	}

	public static void ToggleFocus(bool state)
	{
		SetRTPCValue("InternalVolume", state ? internalvolume : 0f);
	}

	public static void SetFocus(float volume)
	{
		internalvolume = volume;
		SetRTPCValue("InternalVolume", volume);
	}

	public static void SetMasterVolume(float volume)
	{
		SetRTPCValue("MasterVolume", volume);
		MDNPlayerPrefs.PLAYERPREFS_KEY_MASTERVOLUME = volume;
	}

	public static void SetAmbienceVolume(float volume)
	{
		SetRTPCValue("AmbientVolume", volume);
		MDNPlayerPrefs.PLAYERPREFS_KEY_AMBIENCEVOLUME = volume;
	}

	public static void SetSFXVolume(float volume)
	{
		SetRTPCValue("SFXVolume", volume);
		MDNPlayerPrefs.PLAYERPREFS_KEY_SFXVOLUME = volume;
	}

	public static void SetVOVolume(float volume)
	{
		SetRTPCValue("VOVolume", volume);
		MDNPlayerPrefs.PLAYERPREFS_KEY_VOVOLUME = volume;
	}

	public static void SetMusicVolume(float volume)
	{
		SetRTPCValue("MusicVolume", volume);
		MDNPlayerPrefs.PLAYERPREFS_KEY_MUSICVOLUME = volume;
	}

	public static float GetAmbienceVolume()
	{
		return MDNPlayerPrefs.PLAYERPREFS_KEY_AMBIENCEVOLUME;
	}

	public static float GetMasterVolume()
	{
		return MDNPlayerPrefs.PLAYERPREFS_KEY_MASTERVOLUME;
	}

	public static float GetMusicVolume()
	{
		return MDNPlayerPrefs.PLAYERPREFS_KEY_MUSICVOLUME;
	}

	public static float GetVOVolume()
	{
		return MDNPlayerPrefs.PLAYERPREFS_KEY_VOVOLUME;
	}

	public static float GetSFXVolume()
	{
		return MDNPlayerPrefs.PLAYERPREFS_KEY_SFXVOLUME;
	}

	public static void SetRTPCValue(string name, float value)
	{
		if (AkSoundEngine.SetRTPCValue(name, value) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to set RTPC: {0} to the value {1}", name, value.ToString());
		}
	}

	public static void SetRTPCValue(string name, float value, GameObject obj, int direction, AkCurveInterpolation curve)
	{
		if (AkSoundEngine.SetRTPCValue(name, value, obj, direction, curve) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to set RTPC: {0} to the value {1}", name, value.ToString());
		}
	}

	public static void SetRTPCValue(string name, float value, GameObject obj)
	{
		if (AkSoundEngine.SetRTPCValue(name, value, obj) != AKRESULT.AK_Success)
		{
			Debug.LogWarningFormat("AudioManager: Failed to set RTPC: {0} to the value {1}", name, value.ToString());
		}
	}

	public static void PlayAudio(WwiseEvents eventName, GameObject obj, float delay)
	{
		if (delay == 0f)
		{
			PostEvent(eventName.EventName, obj);
		}
		else
		{
			AddPendingEvent(new PendingEvent(delay, eventName.EventName, obj));
		}
	}

	public static void PlayAudio(string eventName, GameObject obj, float delay)
	{
		if (delay == 0f)
		{
			PostEvent(eventName, obj);
		}
		else
		{
			AddPendingEvent(new PendingEvent(delay, eventName, obj));
		}
	}

	public static bool PostEvent(string eventName, GameObject obj, uint callbackFlags = 0u, AkCallbackManager.EventCallback callbackHandler = null)
	{
		AudioHistoryEntry entry = new AudioHistoryEntry
		{
			_event = eventName,
			objectname = ((obj == null) ? "null" : obj.name),
			time = Time.time
		};
		if (((callbackHandler != null) ? AkSoundEngine.PostEvent(eventName, obj, callbackFlags, callbackHandler, null) : AkSoundEngine.PostEvent(eventName, obj)) == 0)
		{
			Debug.LogWarningFormat("AudioManager: Failed to PostEvent: {0} on Object: {1}", eventName, (obj == null) ? "null" : obj.name);
			entry.failed = true;
			if (_instance != null)
			{
				_instance.eventlog.Enqueue(entry);
			}
			return false;
		}
		entry.failed = false;
		if (_instance != null)
		{
			_instance.eventlog.Enqueue(entry);
		}
		return true;
	}

	private static bool UnLoadPck(string pckname)
	{
		AKRESULT aKRESULT = AKRESULT.AK_Success;
		AudioHistoryEntry entry = new AudioHistoryEntry
		{
			_event = "UnLoad",
			objectname = pckname,
			time = Time.time
		};
		aKRESULT = AkSoundEngine.UnloadFilePackage(AkSoundEngine.GetIDFromString(pckname));
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError($"Couldn't unload audio package {pckname} result was {aKRESULT.ToString()}");
			entry.failed = true;
			_instance.eventlog.Enqueue(entry);
			return false;
		}
		if ((bool)_instance)
		{
			if (_instance.loadedPcks.Contains(pckname))
			{
				_instance.loadedPcks.Remove(pckname);
			}
			entry.failed = false;
			_instance.eventlog.Enqueue(entry);
		}
		return true;
	}

	private static bool LoadPck(string pckname)
	{
		AKRESULT aKRESULT = AKRESULT.AK_Success;
		AudioHistoryEntry entry = new AudioHistoryEntry
		{
			_event = "Load",
			objectname = pckname,
			time = Time.time
		};
		uint out_uPackageID = 0u;
		foreach (string audioPackageBasePath in AssetLoader.GetAudioPackageBasePaths())
		{
			AkSoundEngine.AddBasePath(audioPackageBasePath);
		}
		aKRESULT = AkSoundEngine.LoadFilePackage(pckname, out out_uPackageID);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError($"Couldn't load audio package {pckname} result was {aKRESULT.ToString()}");
			entry.failed = true;
			_instance.eventlog.Enqueue(entry);
			return false;
		}
		if ((bool)_instance)
		{
			_instance.loadedPcks.Add(pckname);
			entry.failed = false;
			_instance.eventlog.Enqueue(entry);
		}
		return true;
	}

	private static bool UnLoadBnk(string Bnkname)
	{
		AKRESULT aKRESULT = AKRESULT.AK_Success;
		AudioHistoryEntry entry = new AudioHistoryEntry
		{
			_event = "UnLoad",
			objectname = Bnkname,
			time = Time.time
		};
		aKRESULT = AkSoundEngine.UnloadBank(Bnkname, IntPtr.Zero, null, null);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError($"Couldn't unload audio bank {Bnkname} result was {aKRESULT.ToString()}");
			entry.failed = true;
			_instance.eventlog.Enqueue(entry);
			return false;
		}
		if ((bool)_instance)
		{
			if (_instance.loadedBnks.Contains(Bnkname))
			{
				_instance.loadedBnks.Remove(Bnkname);
			}
			entry.failed = false;
			_instance.eventlog.Enqueue(entry);
		}
		return true;
	}

	private static bool LoadBnk(string Bnkname)
	{
		AKRESULT aKRESULT = AKRESULT.AK_Success;
		uint out_bankID = 0u;
		AudioHistoryEntry entry = new AudioHistoryEntry
		{
			_event = "Load",
			objectname = Bnkname,
			time = Time.time
		};
		aKRESULT = AkSoundEngine.LoadBank(Bnkname, out out_bankID);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			if (aKRESULT != AKRESULT.AK_BankAlreadyLoaded)
			{
				MDNPlayerPrefs.HashAllFilesOnStartup = true;
				ResourceErrorLogger.LogAssetBundleError(null, "Failed to load audio bnk", new Dictionary<string, string>
				{
					{ "BnkName", Bnkname },
					{
						"Result",
						aKRESULT.ToString()
					}
				});
			}
			Debug.LogError($"Couldn't load audio bank {Bnkname} result was {aKRESULT.ToString()}");
			entry.failed = true;
			_instance.eventlog.Enqueue(entry);
			return false;
		}
		if ((bool)_instance)
		{
			_instance.loadedBnks.Add(Bnkname);
			entry.failed = false;
			_instance.eventlog.Enqueue(entry);
		}
		return true;
	}

	public void UnLoadBattleField()
	{
		if (!string.IsNullOrEmpty(loadedBatPck))
		{
			UnLoadPck(loadedBatPck);
			loadedBatPck = string.Empty;
		}
		if (!string.IsNullOrEmpty(loadedBatBnk))
		{
			UnLoadBnk(loadedBatBnk);
			loadedBatBnk = string.Empty;
		}
	}

	public void LoadBattleField(string audioBankName)
	{
		if (!string.IsNullOrEmpty(loadedBatBnk))
		{
			UnLoadBattleField();
		}
		string text = AvailableAudioPackages.FirstOrDefault((string name) => name.Contains(audioBankName));
		if (text != null)
		{
			LoadPck(text);
			loadedBatPck = text;
			text = GetBankNameFromPckName(text);
			LoadBnk(text);
			loadedBatBnk = text;
		}
	}

	public static void UnInitializeAudio()
	{
		AkSoundEngine.StopAll();
		ResetBanks();
		AkSoundEngine.UnloadAllFilePackages();
		if ((bool)_instance)
		{
			_instance.loadedBatBnk = string.Empty;
			_instance.loadedBatPck = string.Empty;
			_instance.loadedPcks.Clear();
			_instance.loadedBnks.Clear();
			_instance.initialized = false;
		}
	}

	public static void InitializeAudioManager(AssetLookupSystem als, IStorageContext storageContext)
	{
		UnInitializeAudio();
		if ((bool)_instance)
		{
			_instance._musicIsPlaying = false;
			_instance._assetLookupSystem = als;
		}
		AudioListener.volume = 0f;
		_storageContext = storageContext;
		foreach (string audioPackageBasePath in AssetLoader.GetAudioPackageBasePaths())
		{
			AkSoundEngine.AddBasePath(audioPackageBasePath);
		}
		LoadBnk("STARTUP");
		if (_instance != null)
		{
			_instance.initialized = true;
			SetMusicVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_MUSICVOLUME);
			SetMasterVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_MASTERVOLUME);
			SetAmbienceVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_AMBIENCEVOLUME);
			SetSFXVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_SFXVOLUME);
			SetVOVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_VOVOLUME);
		}
	}

	public static void InitializeAudio(AssetLookupSystem als)
	{
		UnInitializeAudio();
		if ((bool)_instance)
		{
			_instance._musicIsPlaying = false;
			_instance._assetLookupSystem = als;
		}
		AkSoundEngine.SetCurrentLanguage("English(US)");
		foreach (string availableAudioPackage in AvailableAudioPackages)
		{
			string text = GetBankNameFromPckName(availableAudioPackage)?.ToUpper();
			if (!string.IsNullOrEmpty(text) && !_instance.loadedBnks.Contains(text) && ShouldLoadPck(text) && LoadPck(availableAudioPackage))
			{
				LoadBnk(text.ToUpper());
			}
		}
		if (_instance != null)
		{
			_instance.initialized = true;
			_instance.SwitchLanguage(Languages.CurrentLanguage);
			SetMusicVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_MUSICVOLUME);
			SetMasterVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_MASTERVOLUME);
			SetAmbienceVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_AMBIENCEVOLUME);
			SetSFXVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_SFXVOLUME);
			SetVOVolume(MDNPlayerPrefs.PLAYERPREFS_KEY_VOVOLUME);
		}
	}

	public void SwitchLanguage(string language)
	{
		AkSoundEngine.SetCurrentLanguage(Languages.MTGAtoWwiseMapping[language]);
		if (loadedPcks.FirstOrDefault((string x) => x.Contains("NPE_VO")) != null && loadedBnks.FirstOrDefault((string x) => x.Contains("NPE_VO")) != null)
		{
			string pckname = loadedPcks.Find("NPE_VO", (string x, string paramName) => x.Contains(paramName));
			UnLoadBnk(loadedBnks.Find("NPE_VO", (string x, string paramName) => x.Contains(paramName)));
			UnLoadPck(pckname);
		}
		string newVO = $"NPE_VO_{Languages.MTGAtoI2LangCode[language]}";
		string text = AvailableAudioPackages.FirstOrDefault((string name) => name.Contains(newVO, caseIndependent: true));
		if (string.IsNullOrEmpty(text))
		{
			text = AvailableAudioPackages.FirstOrDefault((string name) => name.Contains("NPE_VO") && name.Count((char y) => y == '_') < 3);
		}
		if (text != null)
		{
			LoadPck(text);
		}
		LoadBnk("NPE_VO");
	}

	public string GetColorKey(CardFilterType color)
	{
		string text = "nill";
		return color switch
		{
			CardFilterType.White => "white", 
			CardFilterType.Blue => "blue", 
			CardFilterType.Black => "black", 
			CardFilterType.Red => "red", 
			CardFilterType.Green => "green", 
			_ => "gold", 
		};
	}

	public static string GetColorKey(ManaColor color)
	{
		string text = "nill";
		return color switch
		{
			ManaColor.White => "white", 
			ManaColor.Blue => "blue", 
			ManaColor.Black => "black", 
			ManaColor.Red => "red", 
			ManaColor.Green => "green", 
			_ => "gold", 
		};
	}

	public string GetColorKey(CardFrameKey color)
	{
		string result = "nill";
		switch (color)
		{
		case CardFrameKey.White:
			result = "white";
			break;
		case CardFrameKey.Blue:
			result = "blue";
			break;
		case CardFrameKey.Black:
			result = "black";
			break;
		case CardFrameKey.Red:
			result = "red";
			break;
		case CardFrameKey.Green:
			result = "green";
			break;
		case CardFrameKey.Gold:
			result = "gold";
			break;
		case CardFrameKey.Colorless:
			result = "gold";
			break;
		}
		return result;
	}

	public void PlayRandomLand()
	{
		string[] array = new string[7] { "sfx_land_basic_plane", "sfx_land_basic_mountain", "sfx_land_basic_swamp", "sfx_land_basic_waterfall", "sfx_land_basic_desert", "sfx_land_basic_island", "sfx_land_basic_forest" };
		PlayAudio(array[UnityEngine.Random.Range(0, array.Length)], Default);
	}

	public static string[] BuildETBAudioEvents(List<string> subtypes)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < subtypes.Count; i++)
		{
			list.Add("sfx_c_type_etb_" + subtypes[i].ToLower());
		}
		return list.ToArray();
	}

	private static bool ShouldLoadPck(string bnkName)
	{
		int num = bnkName.Count((char x) => x == '_');
		return bnkName.IndexOf("INIT") == -1 && bnkName.IndexOf("BAT") == -1 && num <= 1;
	}

	private static string GetBankNameFromPckName(string bnkName)
	{
		return BankNameExtractorRegex.Match(bnkName).Groups[1].Value;
	}

	public static void UnloadAllBanks()
	{
		if (!(Instance != null))
		{
			return;
		}
		foreach (string loadedBnk in Instance.loadedBnks)
		{
			UnLoadBnk(loadedBnk);
		}
	}

	public static void ResetBanks()
	{
		if (AkSoundEngine.IsInitialized())
		{
			AkSoundEngine.ClearBanks();
			AkBankManager.UnloadAllBanks();
			uint out_bankID;
			AKRESULT aKRESULT = AkSoundEngine.LoadBank("Init.bnk", out out_bankID);
			if (aKRESULT != AKRESULT.AK_Success)
			{
				Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + aKRESULT);
			}
		}
	}
}
