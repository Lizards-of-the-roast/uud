using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class PacketSelectDebugScene : MonoBehaviour
{
	public class DebugInterface : IServiceInterface
	{
		private readonly System.Random _rng = new System.Random();

		private string[] _packNames = new string[46]
		{
			"JMP_M21_Basri", "JMP_M21_Unicorns", "JMP_M21_Dogs", "JMP_M21_Enchanted", "JMP_M21_Angels", "JMP_M21_Feathered_Friends", "JMP_M21_Legion", "JMP_M21_Doctor", "JMP_M21_Heavily_Armored", "JMP_M21_Teferi",
			"JMP_M21_Milling", "JMP_M21_Spirits", "JMP_M21_Under_the_Sea", "JMP_M21_Pirates", "JMP_M21_Well_Read", "JMP_M21_Wizards", "JMP_M21_Above_the_Clouds", "JMP_M21_Archaeology", "JMP_M21_Liliana", "JMP_M21_Phyrexian",
			"JMP_M21_Rogues", "JMP_M21_Witchcraft", "JMP_M21_Discarding", "JMP_M21_Minions", "JMP_M21_Vampires", "JMP_M21_Spooky", "JMP_M21_Reanimated", "JMP_M21_Chandra", "JMP_M21_Seismic", "JMP_M21_Minotaurs",
			"JMP_M21_Lightning", "JMP_M21_Dragons", "JMP_M21_Goblins", "JMP_M21_Smashing", "JMP_M21_Spellcasting", "JMP_M21_Devilish", "JMP_M21_Garruk", "JMP_M21_Walls", "JMP_M21_Cats", "JMP_M21_Elves",
			"JMP_M21_Lands", "JMP_M21_Predatory", "JMP_M21_Plus_One", "JMP_M21_Dinosaurs", "JMP_M21_Tree_Hugging", "JMP_M21_Rainbow"
		};

		public ServiceState State { get; private set; }

		public Action<ServiceState> StateUpdated { get; set; }

		public DebugInterface()
		{
			State = new ServiceState(new PacketDetails[2], new PacketDetails[3]
			{
				GenerateRandomPack(),
				GenerateRandomPack(),
				GenerateRandomPack()
			});
		}

		public void Reset(bool partial)
		{
			PacketDetails[] array = new PacketDetails[2];
			if (partial)
			{
				array[0] = GenerateRandomPack();
			}
			PacketDetails[] options = new PacketDetails[3]
			{
				GenerateRandomPack(),
				GenerateRandomPack(),
				GenerateRandomPack()
			};
			State = new ServiceState(array, options);
			StateUpdated?.Invoke(State);
		}

		public void SubmitPack(string packId)
		{
			if (State.CanSubmit(packId))
			{
				PacketDetails optionById = State.GetOptionById(packId);
				PacketDetails[] submittedPackets = State.SubmittedPackets;
				PacketDetails[] array = new PacketDetails[2];
				for (int i = 0; i < submittedPackets.Length; i++)
				{
					if (submittedPackets[i].Equals(default(PacketDetails)))
					{
						array[i] = optionById;
						break;
					}
					array[i] = submittedPackets[i];
				}
				PacketDetails[] array2 = new PacketDetails[3];
				State = new ServiceState(array, array2);
				if (!State.AllPacketsSubmitted())
				{
					for (int j = 0; j < array2.Length; j++)
					{
						array2[j] = GenerateRandomPack();
					}
					State = new ServiceState(array, array2);
				}
				StateUpdated?.Invoke(State);
				return;
			}
			throw new Exception("Cannot submit " + packId);
		}

		private PacketDetails GenerateRandomPack()
		{
			int num = _rng.Next(0, 6);
			string[] array = new string[num];
			List<string> list = new List<string> { "B", "G", "C", "R", "U", "W" };
			for (int i = 0; i < num; i++)
			{
				int index = _rng.Next(0, list.Count);
				array[i] = list[index];
				list.RemoveAt(index);
			}
			return new PacketDetails(RandomPackName(), Guid.NewGuid().ToString(), 7153u, (uint)_rng.Next(10000, 99999), array);
		}

		private string RandomPackName()
		{
			return _packNames[_rng.Next(_packNames.Length)];
		}
	}

	[Serializable]
	public class DebugArtProvider : IPacketArtProvider
	{
		private IArtCropProvider _cropDataBase;

		[SerializeField]
		private int[] _artIds;

		[SerializeField]
		private bool _randomArt = true;

		private System.Random _rng = new System.Random();

		private int _texIdx;

		public PacketArt GetPacketArt(AssetTracker assetTracker, string assetTrackingKey, uint artId)
		{
			if (_artIds.Length == 0)
			{
				return new PacketArt(new Texture2D(1, 1), null);
			}
			if (_randomArt)
			{
				string text = ArtPath(_rng.Next(0, _artIds.Length));
				Texture texture = Resources.Load(text) as Texture;
				ArtCrop crop = CropDatabase().GetCrop(text, "Frameless");
				return new PacketArt(texture, crop);
			}
			string artPath = ArtPath(_artIds[_texIdx]);
			Texture texture2 = null;
			if (texture2 == null)
			{
				texture2 = new Texture2D(1, 1);
			}
			ArtCrop crop2 = CropDatabase().GetCrop(artPath, "Frameless");
			_texIdx++;
			if (_texIdx >= _artIds.Length)
			{
				_texIdx = 0;
			}
			return new PacketArt(texture2, crop2);
		}

		private string ArtPath(int artId)
		{
			return $"Assets/Core/CardArt/{artId}_AIF.tga";
		}

		private IArtCropProvider CropDatabase()
		{
			return _cropDataBase ?? (_cropDataBase = ArtCropDatabaseUtils.LoadBestProvider(NullBILogger.Default));
		}
	}

	[SerializeField]
	private PacketSelectContentController _contentController;

	[SerializeField]
	private DebugArtProvider _artProvider;

	private bool _renderDebugUI;

	private DebugInterface _debugService = new DebugInterface();

	private ServiceState _currentState = new ServiceState(new PacketDetails[0], new PacketDetails[0]);

	private Rect areaRect = new Rect(20f, 20f, 230f, 500f);

	private void Start()
	{
		_contentController.Init(null, new NullCardDataProvider(), null, null, _artProvider, null);
		_contentController.SetInterface(_debugService);
		DebugInterface debugService = _debugService;
		debugService.StateUpdated = (Action<ServiceState>)Delegate.Combine(debugService.StateUpdated, new Action<ServiceState>(OnStateUpdated));
	}

	private void OnStateUpdated(ServiceState state)
	{
		_currentState = state;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			_renderDebugUI = !_renderDebugUI;
		}
	}

	private void OnDestroy()
	{
		if (_debugService != null)
		{
			DebugInterface debugService = _debugService;
			debugService.StateUpdated = (Action<ServiceState>)Delegate.Remove(debugService.StateUpdated, new Action<ServiceState>(OnStateUpdated));
		}
	}

	private void OnGUI()
	{
		if (!_renderDebugUI)
		{
			return;
		}
		GUILayout.BeginArea(areaRect, GUI.skin.box);
		if (GUILayout.Button("Reset - Partial"))
		{
			_debugService.Reset(partial: true);
			_contentController.SetInterface(_debugService);
		}
		if (GUILayout.Button("Reset - Full"))
		{
			_debugService.Reset(partial: false);
			_contentController.SetInterface(_debugService);
		}
		GUILayout.Space(5f);
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label("Submitted Packs");
		for (int i = 0; i < _currentState.SubmittedPackets.Length; i++)
		{
			PacketDetails packetDetails = _currentState.SubmittedPackets[i];
			if (packetDetails.Equals(default(PacketDetails)))
			{
				GUILayout.Label("PACK " + (i + 1));
				continue;
			}
			GUI.color = Color.green;
			renderPackDetails(_currentState.SubmittedPackets[i]);
			GUI.color = Color.white;
		}
		GUILayout.EndVertical();
		GUILayout.Space(5f);
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label("Packet Options");
		for (int j = 0; j < _currentState.PacketOptions.Length; j++)
		{
			renderPackDetails(_currentState.PacketOptions[j]);
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
		static void renderPackDetails(PacketDetails packDetails)
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label(packDetails.Name);
			GUILayout.Label(packDetails.PacketId);
			GUILayout.EndVertical();
		}
	}
}
