using Core.Code.Promises;
using Core.Meta.MainNavigation.SocialV2;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;
using _3rdParty.ExternalChat;

namespace Core.Shared.Code.DebugTools;

public class DebugGatheringWindowGUI : IDebugGUIPage
{
	private readonly GatheringManager _gatheringManager = Pantry.Get<GatheringManager>();

	private string newGatheringName = "";

	private string existingGatheringId = "";

	private string existingGatheringPassword = "";

	private IExternalChatManager _externalChatManager;

	private bool discordConnected;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Gatherings;

	public string TabName => "Gatherings";

	public bool HiddenInTab => false;

	public DebugGatheringWindowGUI(IExternalChatManager externalChatManager)
	{
		_externalChatManager = externalChatManager;
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		GUILayout.BeginVertical();
		if (!discordConnected)
		{
			DisplayAuthTokenButton();
		}
		else
		{
			DisplayGatherings();
			DisplayJoinNewGatheringButton();
			DisplayJoinExistingGatheringButton();
		}
		GUILayout.EndHorizontal();
	}

	private void DisplayGatherings()
	{
		foreach (Core.Meta.MainNavigation.SocialV2.Gathering gathering in _gatheringManager.Gatherings)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Name: " + gathering.Name + " | Owner Id: " + gathering.OwnerId + " | Gathering Id: ");
			GUILayout.TextField(gathering.Id);
			if (GUILayout.Button("Leave Gathering", GUILayout.Width(150f)))
			{
				_gatheringManager.LeaveGathering(gathering.Id);
				newGatheringName = "";
			}
			GUILayout.EndHorizontal();
		}
	}

	private void DisplayAuthTokenButton()
	{
		if (!GUILayout.Button("Ask Services for AuthToken", GUILayout.Width(200f)) || _externalChatManager == null)
		{
			return;
		}
		_externalChatManager.RequestAuth().ThenOnMainThreadIfSuccess(delegate(DiscordBotAuthResp resp)
		{
			_externalChatManager.OnAuthResponse(resp);
			if (resp != null)
			{
				discordConnected = true;
				_gatheringManager.UpdateGatherings();
			}
		});
	}

	private void DisplayJoinNewGatheringButton()
	{
		GUILayout.BeginHorizontal();
		newGatheringName = GUILayout.TextField(newGatheringName, GUILayout.Width(300f));
		if (GUILayout.Button("Create Gathering", GUILayout.Width(150f)))
		{
			_gatheringManager.CreateGathering(newGatheringName, "password");
			newGatheringName = "";
		}
		GUILayout.EndHorizontal();
	}

	private void DisplayJoinExistingGatheringButton()
	{
		GUILayout.BeginHorizontal();
		existingGatheringId = GUILayout.TextField(existingGatheringId, GUILayout.Width(300f));
		if (GUILayout.Button("Join Gathering", GUILayout.Width(150f)))
		{
			_gatheringManager.JoinGathering(existingGatheringId);
			existingGatheringId = "";
		}
		GUILayout.EndHorizontal();
	}
}
