using System;
using System.Collections.Generic;
using GreClient.CardData;
using MovementSystem;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga.Logging;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class PacketSelectContentController : NavContentController
{
	[Serializable]
	private class CardLayout
	{
		[SerializeField]
		private Transform _center;

		[SerializeField]
		private float _size = 1f;

		[SerializeField]
		private Vector3 _spacing = new Vector3(0f, 0f, 0f);

		public Transform Center => _center;

		public float Size => _size;

		public Vector3 Spacing => _spacing;

		public Vector3 PositionForIndex(int idx, int cardCount)
		{
			Vector3 result = -0.5f * Spacing * (cardCount - 1) + _center.position;
			for (int i = 0; i < idx; i++)
			{
				result += Spacing;
			}
			return result;
		}
	}

	[SerializeField]
	private CustomButton _confirmSelectionButton;

	[SerializeField]
	private Localize _headerText;

	[Space(5f)]
	[SerializeField]
	private SplineMovementData _transitionSpline;

	[SerializeField]
	private JumpStartPacket _packPrefab;

	[SerializeField]
	private float _hoverDistance = 20f;

	[SerializeField]
	private float _hoverScale = 1.1f;

	[SerializeField]
	private FaceHanger _faceHanger;

	[SerializeField]
	private SplineMovementData _hoverSpline;

	private JumpStartPacket _hoveredPack;

	private ICardDataProvider _cardDatabase = new NullCardDataProvider();

	[Space(5f)]
	[Header("Pack Layout")]
	[SerializeField]
	private CardLayout _selectedPackLayout;

	[SerializeField]
	private CardLayout _packSelectionLayout;

	[SerializeField]
	private CardLayout _faceHangerLayout;

	private List<JumpStartPacket> _submittedPacks = new List<JumpStartPacket>();

	private List<JumpStartPacket> _packetOptions = new List<JumpStartPacket>();

	private Dictionary<JumpStartPacket, string> _packetToId = new Dictionary<JumpStartPacket, string>();

	private Dictionary<string, JumpStartPacket> _idToPacket = new Dictionary<string, JumpStartPacket>();

	private string _selectedPackId = string.Empty;

	private IServiceInterface _serviceInterface = new NullServiceInterface();

	private ServiceState _currentState = new ServiceState(new PacketDetails[0], new PacketDetails[0]);

	private IPacketArtProvider _packetArtProvider = new NullPacketArtProvider();

	private IPacketAudioProvider _packetAudioProvider = new NullPacketAudioProvider();

	private ISplineMovementSystem _movementSystem = new SplineMovementSystem();

	private IClientLocProvider _locManager = NullLocProvider.Default;

	private UXEventQueue _eventQueue = new UXEventQueue();

	private InventoryManager _invManager;

	private Vector3 _hangerOriginalScale;

	private Wizards.Arena.Client.Logging.ILogger _logger;

	private AssetTracker _assetTracker = new AssetTracker();

	public override NavContentType NavContentType => NavContentType.PacketSelect;

	public void Init(IClientLocProvider locManager, ICardDataProvider cardDatabase, CardViewBuilder cardBuilder, InventoryManager invManager, IPacketArtProvider packetArtProvider, IPacketAudioProvider packetAudioProvider, Wizards.Arena.Client.Logging.ILogger logger = null)
	{
		_locManager = locManager ?? NullLocProvider.Default;
		_cardDatabase = cardDatabase ?? new NullCardDataProvider();
		_invManager = invManager;
		_packetArtProvider = packetArtProvider ?? new NullPacketArtProvider();
		_packetAudioProvider = packetAudioProvider ?? new NullPacketAudioProvider();
		if (cardBuilder != null && _faceHanger != null)
		{
			_faceHanger.Cleanup();
			_faceHanger.Init(new LandFaceGenerator(_locManager, _invManager), cardBuilder);
			_hangerOriginalScale = _faceHanger.transform.localScale;
		}
		else
		{
			_faceHanger = null;
		}
		_logger = logger;
		if (_logger == null)
		{
			_logger = new UnityLogger("PacketSelectContentController", LoggerLevel.Error);
			LoggerManager.Register((Wizards.Arena.Client.Logging.Logger)_logger);
		}
		_confirmSelectionButton.OnClick.AddListener(delegate
		{
			SubmitPack(_selectedPackId);
		});
	}

	public void SetInterface(IServiceInterface serviceInterface)
	{
		ClearService();
		_serviceInterface = serviceInterface;
		IServiceInterface serviceInterface2 = _serviceInterface;
		serviceInterface2.StateUpdated = (Action<ServiceState>)Delegate.Combine(serviceInterface2.StateUpdated, new Action<ServiceState>(OnStateUpdated));
		OnStateUpdated(_serviceInterface.State);
	}

	private void Update()
	{
		_movementSystem?.UpdateMovement();
		_eventQueue?.Update(Time.deltaTime);
	}

	private void OnStateUpdated(ServiceState newModel)
	{
		_eventQueue.EnqueuePending(new CallbackUXEvent(delegate
		{
			SetServiceState(newModel);
		}, _logger));
	}

	private void SetServiceState(ServiceState newState)
	{
		_currentState = newState;
		SetHeaderText(_currentState.SubmissionCount());
		_packetToId.Clear();
		_idToPacket.Clear();
		ClearSelectablePacks();
		ResetPackSelection();
		SetPacketSubmissions(_currentState.SubmittedPackets);
		SetPacketOptions(_currentState.PacketOptions);
	}

	private void SetPacketSubmissions(PacketDetails[] submittedPacks)
	{
		for (int i = 0; i < submittedPacks.Length; i++)
		{
			PacketDetails details = submittedPacks[i];
			if (details.Equals(default(PacketDetails)))
			{
				continue;
			}
			if (i == 0 && _submittedPacks.Count == 0)
			{
				JumpStartPacket jumpStartPacket = UnityEngine.Object.Instantiate(_packPrefab, base.transform);
				PacketInput input = jumpStartPacket.Input;
				input.Clicked = (Action<JumpStartPacket>)Delegate.Combine(input.Clicked, new Action<JumpStartPacket>(OnSubmittedPackClicked));
				input.DoubleClicked = (Action<JumpStartPacket>)Delegate.Combine(input.DoubleClicked, new Action<JumpStartPacket>(OnSubmittedPackClicked));
				input.MouseEntered = (Action<JumpStartPacket>)Delegate.Combine(input.MouseEntered, new Action<JumpStartPacket>(OnCardMouseEnter));
				input.MouseExit = (Action<JumpStartPacket>)Delegate.Combine(input.MouseExit, new Action<JumpStartPacket>(OnCardMouseExit));
				if (PlatformUtils.IsHandheld())
				{
					input.BeginClickAndHold = (Action<JumpStartPacket>)Delegate.Combine(input.BeginClickAndHold, new Action<JumpStartPacket>(OnCardClickAndHold));
					input.EndClickAndHold = (Action<JumpStartPacket>)Delegate.Combine(input.EndClickAndHold, new Action<JumpStartPacket>(OnCardMouseExit));
				}
				_submittedPacks.Add(jumpStartPacket);
				Vector3 vector = _selectedPackLayout.PositionForIndex(0, 1);
				Transform root = jumpStartPacket.Root;
				root.position = vector;
				_movementSystem.AddPermanentGoal(root, new IdealPoint(vector, Quaternion.identity, Vector3.one * _selectedPackLayout.Size));
			}
			if (i < _submittedPacks.Count)
			{
				JumpStartPacket jumpStartPacket2 = _submittedPacks[i];
				jumpStartPacket2.SetName(PacketLocKey(details.Name));
				jumpStartPacket2.SetPacketArt(_packetArtProvider.GetPacketArt(_assetTracker, details.Name, details.ArtId));
				SetPacketColorDisplay(jumpStartPacket2, details);
			}
		}
		ClearSubmittedPacks((int)_currentState.SubmissionCount());
		UpdateBanners();
	}

	private void SetPacketOptions(PacketDetails[] options)
	{
		for (int i = 0; i < options.Length; i++)
		{
			PacketDetails details = options[i];
			if (!details.Equals(default(PacketDetails)))
			{
				JumpStartPacket jumpStartPacket = UnityEngine.Object.Instantiate(_packPrefab, base.transform);
				jumpStartPacket.SetName(PacketLocKey(details.Name));
				jumpStartPacket.SetPacketArt(_packetArtProvider.GetPacketArt(_assetTracker, details.Name, details.ArtId));
				SetPacketColorDisplay(jumpStartPacket, details);
				PacketInput input = jumpStartPacket.Input;
				input.Clicked = (Action<JumpStartPacket>)Delegate.Combine(input.Clicked, new Action<JumpStartPacket>(OnPacketClicked));
				input.DoubleClicked = (Action<JumpStartPacket>)Delegate.Combine(input.DoubleClicked, new Action<JumpStartPacket>(OnPacketDoubleClicked));
				input.MouseEntered = (Action<JumpStartPacket>)Delegate.Combine(input.MouseEntered, new Action<JumpStartPacket>(OnCardMouseEnter));
				input.MouseExit = (Action<JumpStartPacket>)Delegate.Combine(input.MouseExit, new Action<JumpStartPacket>(OnCardMouseExit));
				if (PlatformUtils.IsHandheld())
				{
					input.BeginClickAndHold = (Action<JumpStartPacket>)Delegate.Combine(input.BeginClickAndHold, new Action<JumpStartPacket>(OnCardClickAndHold));
					input.EndClickAndHold = (Action<JumpStartPacket>)Delegate.Combine(input.EndClickAndHold, new Action<JumpStartPacket>(OnCardMouseExit));
				}
				Vector3 vector = _packSelectionLayout.PositionForIndex(i, options.Length);
				Transform root = jumpStartPacket.Root;
				root.position = vector;
				_movementSystem.AddPermanentGoal(root, new IdealPoint(vector, Quaternion.identity, Vector3.one * _packSelectionLayout.Size));
				string packetId = details.PacketId;
				_packetToId[jumpStartPacket] = packetId;
				_idToPacket[packetId] = jumpStartPacket;
				_packetOptions.Add(jumpStartPacket);
			}
		}
	}

	public void SetPacketColorDisplay(JumpStartPacket packet, PacketDetails details)
	{
		packet.SetPacketColors(details.RawColors);
	}

	private void SelectPack(string packId)
	{
		if (!string.IsNullOrEmpty(packId) && _currentState.CanSubmit(packId) && !_currentState.AllPacketsSubmitted())
		{
			_selectedPackId = packId;
		}
	}

	private void TogglePacket(string packId)
	{
		if (!string.IsNullOrEmpty(packId) && _currentState.CanSubmit(packId) && !_currentState.AllPacketsSubmitted())
		{
			_selectedPackId = ((_selectedPackId != packId) ? packId : string.Empty);
		}
	}

	private void SubmitPack(string packIdToSubmit)
	{
		if (string.IsNullOrEmpty(packIdToSubmit) || _currentState.GetOptionById(packIdToSubmit).Equals(default(PacketDetails)))
		{
			return;
		}
		if (_idToPacket.TryGetValue(packIdToSubmit, out var value))
		{
			_submittedPacks.Add(value);
			_packetOptions.Remove(value);
			PacketInput input = value.Input;
			input.ResetInput();
			input.Clicked = (Action<JumpStartPacket>)Delegate.Combine(input.Clicked, new Action<JumpStartPacket>(OnSubmittedPackClicked));
			input.DoubleClicked = (Action<JumpStartPacket>)Delegate.Combine(input.DoubleClicked, new Action<JumpStartPacket>(OnSubmittedPackClicked));
			input.MouseEntered = (Action<JumpStartPacket>)Delegate.Combine(input.MouseEntered, new Action<JumpStartPacket>(OnCardMouseEnter));
			input.MouseExit = (Action<JumpStartPacket>)Delegate.Combine(input.MouseExit, new Action<JumpStartPacket>(OnCardMouseExit));
			if (PlatformUtils.IsHandheld())
			{
				input.BeginClickAndHold = (Action<JumpStartPacket>)Delegate.Combine(input.BeginClickAndHold, new Action<JumpStartPacket>(OnCardClickAndHold));
				input.EndClickAndHold = (Action<JumpStartPacket>)Delegate.Combine(input.EndClickAndHold, new Action<JumpStartPacket>(OnCardMouseExit));
			}
			if (_currentState.SubmissionCount() == 0)
			{
				Transform root = value.Root;
				_movementSystem.RemoveTemporaryGoal(root);
				Vector3 pos = _selectedPackLayout.PositionForIndex(0, 1);
				IdealPoint endPoint = new IdealPoint(pos, Quaternion.identity, Vector3.one * _packSelectionLayout.Size);
				_movementSystem.AddPermanentGoal(root, endPoint, allowInteractions: true, _transitionSpline);
				ClearSelectablePacks();
				_eventQueue.EnqueuePending(new WaitForSecondsUXEvent(0.3f));
				_eventQueue.EnqueuePending(new WaitUntilUXEvent(() => _movementSystem.GetProgress(root) == 1f));
			}
		}
		_serviceInterface.SubmitPack(packIdToSubmit);
		string packetAudio = _packetAudioProvider.GetPacketAudio(packIdToSubmit);
		AudioManager.PlayAudio((!string.IsNullOrEmpty(packetAudio)) ? packetAudio : WwiseEvents.sfx_ui_accept.EventName, base.gameObject);
		ResetPackSelection();
	}

	private void OnPacketClicked(JumpStartPacket pack)
	{
		if (CanClickPack(pack))
		{
			string packetId = GetPacketId(pack);
			TogglePacket(packetId);
			UpdateClickedPackVisuals(pack);
		}
	}

	private void OnPacketDoubleClicked(JumpStartPacket pack)
	{
		if (CanClickPack(pack))
		{
			string packetId = GetPacketId(pack);
			SelectPack(packetId);
			SubmitPack(_selectedPackId);
			UpdateClickedPackVisuals(pack);
		}
	}

	private bool CanClickPack(JumpStartPacket pack)
	{
		if (_currentState.AllPacketsSubmitted())
		{
			return false;
		}
		if (!_packetOptions.Contains(pack))
		{
			return false;
		}
		if (string.IsNullOrEmpty(GetPacketId(pack)))
		{
			return false;
		}
		return true;
	}

	private void UpdateClickedPackVisuals(JumpStartPacket pack)
	{
		OnCardMouseEnter(pack);
		UpdateHighlights();
		UpdateBanners();
		UpdateButtonState();
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_hightlight_on_selection, pack.gameObject);
	}

	private void OnSubmittedPackClicked(JumpStartPacket pack)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid.EventName, pack.gameObject);
	}

	private void OnCardMouseEnter(JumpStartPacket pack)
	{
		if (_hoveredPack == pack)
		{
			return;
		}
		Transform root = pack.Root;
		if (!_movementSystem.InteractionsAreAllowed(root))
		{
			return;
		}
		PacketDetails detailsById = _currentState.GetDetailsById(GetPacketId(pack));
		if (!detailsById.Equals(default(PacketDetails)))
		{
			_hoveredPack = pack;
			ShowPackHover(root, detailsById);
			if (PlatformUtils.IsDesktop())
			{
				ShowHanger(root, detailsById);
			}
			UpdateHighlights();
		}
	}

	private void OnCardClickAndHold(JumpStartPacket pack)
	{
		Transform root = pack.Root;
		if (_movementSystem.InteractionsAreAllowed(root))
		{
			PacketDetails detailsById = _currentState.GetDetailsById(GetPacketId(pack));
			if (!detailsById.Equals(default(PacketDetails)))
			{
				ShowHanger(root, detailsById, _hoverScale);
			}
		}
	}

	private void ShowPackHover(Transform root, PacketDetails packDetails)
	{
		Vector3 pos = new Vector3(root.position.x, root.position.y, base.transform.position.z - _hoverDistance);
		Vector3 scale = Vector3.one * (_packSelectionLayout.Size * _hoverScale);
		_movementSystem.AddTemporaryGoal(root, new IdealPoint(pos, Quaternion.identity, scale), allowInteractions: true, _hoverSpline);
	}

	private void ShowHanger(Transform root, PacketDetails packDetails, float preAppliedScale = 1f)
	{
		Vector3 vector = new Vector3(root.position.x, root.position.y, base.transform.position.z - _hoverDistance);
		if (_faceHanger != null)
		{
			_faceHanger.transform.position = vector + Vector3.right * _faceHangerLayout.Spacing.x * preAppliedScale;
			_faceHanger.transform.localScale = _hangerOriginalScale * preAppliedScale;
		}
		CardPrintingData cardPrintingById = _cardDatabase.GetCardPrintingById(packDetails.LandGrpId);
		CardData cardData = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
		_faceHanger?.ActivateHanger(cardData, cardData, new HangerSituation
		{
			DelayActivation = true
		});
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, root.gameObject);
	}

	private void OnCardMouseExit(JumpStartPacket cardView)
	{
		_movementSystem.RemoveTemporaryGoal(cardView.Root);
		_hoveredPack = null;
		_faceHanger?.DeactivateHanger();
		UpdateHighlights();
	}

	private void SetHeaderText(uint packNumber)
	{
		_headerText.SetText(key(packNumber));
		static string key(uint num)
		{
			return num switch
			{
				1u => "Events/Packets/Event_Header_First_Packet", 
				2u => "Events/Packets/Event_Header_Second_Packet", 
				_ => "Events/Packets/Event_Header_First_Packet", 
			};
		}
	}

	private void ClearSelectablePacks()
	{
		while (_packetOptions.Count > 0)
		{
			JumpStartPacket jumpStartPacket = _packetOptions[0];
			jumpStartPacket.Input.ResetInput();
			UnityEngine.Object.Destroy(jumpStartPacket.gameObject);
			_packetOptions.RemoveAt(0);
		}
	}

	private void ClearSubmittedPacks(int desiredCount = 0)
	{
		while (_submittedPacks.Count > desiredCount)
		{
			JumpStartPacket jumpStartPacket = _submittedPacks[desiredCount];
			jumpStartPacket.Input.ResetInput();
			UnityEngine.Object.Destroy(jumpStartPacket.gameObject);
			_submittedPacks.RemoveAt(desiredCount);
		}
	}

	private void ResetPackSelection()
	{
		_selectedPackId = string.Empty;
		_confirmSelectionButton.Interactable = false;
		if (_hoveredPack != null)
		{
			_movementSystem.RemoveTemporaryGoal(_hoveredPack.Root);
			_hoveredPack = null;
		}
		UpdateHighlights();
		UpdateBanners();
	}

	private void UpdateHighlights()
	{
		bool flag = _currentState.AllPacketsSubmitted();
		foreach (JumpStartPacket packetOption in _packetOptions)
		{
			packetOption.UpdateHighlight(!flag && _hoveredPack == packetOption);
		}
		foreach (JumpStartPacket submittedPack in _submittedPacks)
		{
			submittedPack?.UpdateHighlight(active: false);
		}
	}

	private void UpdateBanners()
	{
		for (int i = 0; i < _submittedPacks.Count; i++)
		{
			_submittedPacks[i].SetAsSubmittedPacketHeader((uint)(i + 1));
		}
		PacketDetails[] packetOptions = _currentState.PacketOptions;
		for (int j = 0; j < packetOptions.Length; j++)
		{
			PacketDetails packetDetails = packetOptions[j];
			if (packetDetails.Equals(default(PacketDetails)))
			{
				continue;
			}
			string packetId = packetDetails.PacketId;
			if (!string.IsNullOrEmpty(packetId) && _idToPacket.TryGetValue(packetId, out var value) && _packetOptions.Contains(value))
			{
				if (packetId == _selectedPackId)
				{
					value.SetSelectedPacketHeader(_currentState.SubmissionCount() + 1);
				}
				else
				{
					value.ResetBanners();
				}
			}
		}
	}

	private void UpdateButtonState()
	{
		_confirmSelectionButton.Interactable = !string.IsNullOrEmpty(_selectedPackId);
	}

	private void ClearService()
	{
		if (_serviceInterface != null)
		{
			IServiceInterface serviceInterface = _serviceInterface;
			serviceInterface.StateUpdated = (Action<ServiceState>)Delegate.Remove(serviceInterface.StateUpdated, new Action<ServiceState>(OnStateUpdated));
			_serviceInterface = null;
		}
	}

	public override void OnFinishClose()
	{
		ClearService();
		_confirmSelectionButton.OnClick.RemoveAllListeners();
		ClearSelectablePacks();
		ClearSubmittedPacks();
		_assetTracker.Cleanup();
		_idToPacket.Clear();
		_packetToId.Clear();
		_faceHanger?.Cleanup();
		base.OnFinishClose();
	}

	private string GetPacketId(JumpStartPacket packet)
	{
		if (_packetToId.TryGetValue(packet, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	private MTGALocalizedString PacketLocKey(string packetName)
	{
		return new MTGALocalizedString
		{
			Key = "Events/Packets/" + packetName
		};
	}

	private void OnDestroy()
	{
		OnFinishClose();
	}

	private void OnDrawGizmos()
	{
		float magnitude = base.transform.lossyScale.magnitude;
		drawCardCube(_selectedPackLayout.PositionForIndex(0, 1), magnitude * _selectedPackLayout.Size);
		for (int i = 0; i < 3; i++)
		{
			drawCardCube(_packSelectionLayout.PositionForIndex(i, 3), magnitude * _packSelectionLayout.Size);
		}
		static void drawCardCube(Vector3 position, float mag)
		{
			Gizmos.DrawWireCube(position, new Vector3(2.5f, 3.5f, 0.1f) * mag);
		}
	}
}
