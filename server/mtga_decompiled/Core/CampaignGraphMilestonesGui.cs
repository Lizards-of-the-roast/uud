using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;

public class CampaignGraphMilestonesGui : IDebugGUIPage
{
	private CampaignGraphManager _manager;

	private ClientGraphDefinition _npeGraphDef;

	private ClientCampaignGraphState _graphState;

	private List<MilestoneBindings> _milestoneBindings;

	private DebugInfoIMGUIOnGui _GUI;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.CampaignGraphMilestones;

	public string TabName => "Milestones";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
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
		if (_manager == null)
		{
			_manager = Pantry.Get<CampaignGraphManager>();
		}
		if (_npeGraphDef == null)
		{
			_manager.GetDefinitions().Result.TryGetValue("NewPlayerExperience", out var value);
			_npeGraphDef = value;
			if (_npeGraphDef == null)
			{
				SimpleLog.LogError("Graph definition not found for graph id: NewPlayerExperience");
				return;
			}
			_graphState = _manager.GetState(_npeGraphDef);
		}
		if (_npeGraphDef != null && _milestoneBindings == null)
		{
			RefreshMilestoneBindings();
		}
		_GUI.ShowLabel("------- Campaign Graph Milestones -------");
		foreach (MilestoneBindings milestoneBinding in _milestoneBindings)
		{
			GUILayout.BeginVertical();
			milestoneBinding.Completed = _GUI.ShowToggle(milestoneBinding.Completed, milestoneBinding.Name);
			GUILayout.EndVertical();
		}
		if (GUILayout.Button("Update State"))
		{
			if (_graphState == null)
			{
				Dictionary<string, bool> milestoneStates = _npeGraphDef.Milestones.ToDictionary((ClientGraphMilestone def) => def.Name, (ClientGraphMilestone def) => false);
				ClientCampaignGraphState state = new ClientCampaignGraphState
				{
					MilestoneStates = milestoneStates,
					NodeStates = new Dictionary<string, ClientNodeState>()
				};
				_graphState = _manager.UpdateState(_npeGraphDef, state);
			}
			foreach (MilestoneBindings milestoneBinding2 in _milestoneBindings)
			{
				_graphState.MilestoneStates[milestoneBinding2.Name] = milestoneBinding2.Completed;
			}
		}
		if (GUILayout.Button("Refresh State") && _graphState != null && _npeGraphDef != null)
		{
			RefreshMilestoneBindings();
		}
	}

	private void RefreshMilestoneBindings()
	{
		_milestoneBindings = new List<MilestoneBindings>();
		foreach (ClientGraphMilestone milestone in _npeGraphDef.Milestones)
		{
			bool value = false;
			_graphState?.MilestoneStates?.TryGetValue(milestone.Name, out value);
			MilestoneBindings item = new MilestoneBindings
			{
				Name = milestone.Name,
				Completed = value
			};
			_milestoneBindings.Add(item);
		}
	}
}
