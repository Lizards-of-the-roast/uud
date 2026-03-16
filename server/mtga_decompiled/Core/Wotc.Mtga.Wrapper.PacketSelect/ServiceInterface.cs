using System;
using System.Collections.Generic;
using Core.Code.Promises;
using Newtonsoft.Json.Linq;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class ServiceInterface : IServiceInterface
{
	private readonly EventContext _evtContext;

	private readonly SceneLoader _sceneLoader;

	private readonly InventoryManager _invManager;

	private readonly IFrontDoorConnectionServiceWrapper _frontDoorWrapper;

	private JObject _outgoingBIPayload;

	public ServiceState State { get; private set; } = new ServiceState(new PacketDetails[0], new PacketDetails[0]);

	public Action<ServiceState> StateUpdated { get; set; }

	public ServiceInterface(EventContext evtContext, SceneLoader sceneLoader, InventoryManager invManager, IFrontDoorConnectionServiceWrapper frontDoorWrapper)
	{
		_evtContext = evtContext;
		_sceneLoader = sceneLoader;
		_invManager = invManager;
		_frontDoorWrapper = frontDoorWrapper;
		State = TranslateState(_evtContext.PlayerEvent);
	}

	~ServiceInterface()
	{
		State = default(ServiceState);
		StateUpdated = null;
	}

	private ServiceState TranslateState(IPlayerEvent playerEvt)
	{
		if (playerEvt == null)
		{
			return default(ServiceState);
		}
		List<DTO_JumpStartSelection> packetsChosen = playerEvt.PacketsChosen;
		PacketDetails[] array = new PacketDetails[2];
		for (int i = 0; i < packetsChosen.Count && i < array.Length; i++)
		{
			array[i] = TranslateJumpStartSelectionIntoPacketDetails(packetsChosen[i]);
		}
		List<DTO_JumpStartSelection> currentChoices = playerEvt.CurrentChoices;
		PacketDetails[] array2 = new PacketDetails[3];
		if (currentChoices.Count > 0)
		{
			for (int j = 0; j < currentChoices.Count && j < array2.Length; j++)
			{
				array2[j] = TranslateJumpStartSelectionIntoPacketDetails(currentChoices[j]);
			}
		}
		return new ServiceState(array, array2);
	}

	private PacketDetails TranslateJumpStartSelectionIntoPacketDetails(DTO_JumpStartSelection selection)
	{
		return new PacketDetails(selection.packetName, selection.packetName, (uint)selection.displayGrpId, (uint)selection.displayArtId, selection.colors);
	}

	public void SubmitPack(string packId)
	{
		if (State.AllPacketsSubmitted() || State.GetOptionById(packId).Equals(default(PacketDetails)) || _evtContext == null)
		{
			return;
		}
		IPlayerEvent playerEvent = _evtContext.PlayerEvent;
		if (playerEvent != null)
		{
			_outgoingBIPayload = new JObject
			{
				{ "context", "JumpStart.PacketOwnership" },
				{
					"courseId",
					_evtContext.PlayerEvent.CourseData.Id
				},
				{
					"eventId",
					_evtContext.PlayerEvent.EventInfo.InternalEventName
				},
				{ "sessionId", _frontDoorWrapper.SessionId },
				{ "selectedPacketId", packId },
				{
					"pickNumber",
					State.SubmissionCount() + 1
				}
			};
			PacketDetails[] packetOptions = State.PacketOptions;
			for (int i = 0; i < packetOptions.Length; i++)
			{
				PacketDetails packetDetails = packetOptions[i];
				string propertyName = "choice" + (i + 1);
				_outgoingBIPayload[propertyName] = new JObject
				{
					{ "packetId", packetDetails.PacketId },
					{
						"ownership",
						_invManager.Cards.ContainsKey(State.GetDetailsById(packetDetails.PacketId).LandGrpId) ? "Owned" : "Unowned"
					}
				};
			}
			playerEvent.SubmitEventChoice(packId, ChoiceType.JumpStartPacket).ThenOnMainThread(delegate(Promise<ICourseInfoWrapper> p)
			{
				OnSubmitComplete(p.Successful, p.Error);
			});
		}
	}

	private void OnSubmitComplete(bool success, Error error)
	{
		if (success)
		{
			_ = State;
			State = TranslateState(_evtContext.PlayerEvent);
			StateUpdated?.Invoke(State);
			if (State.AllPacketsSubmitted())
			{
				_sceneLoader?.GoToEventScreen(_evtContext);
			}
		}
		else
		{
			_outgoingBIPayload = null;
			_sceneLoader?.GoToLanding(new HomePageContext());
		}
	}
}
