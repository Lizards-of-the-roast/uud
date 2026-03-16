using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.AssetLookupTree.Payloads;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementSet : IClientAchievementSet
{
	private readonly IClientLocProvider _localizationProvider;

	private AssetLookupSystem _assetLookupSystem;

	private readonly CampaignGraphManager _campaignGraphManager;

	private readonly string _graphId;

	private CollationMapping _expansionCode;

	private IList<IClientAchievementGroup> _achievementGroups = new List<IClientAchievementGroup>();

	private ClientGraphDefinition GraphDefinition
	{
		get
		{
			if (!_campaignGraphManager.TryGetGraphDefinition(_graphId, out var graphDefinition))
			{
				Debug.LogErrorFormat("Could not find the graph definition for the graph ID: {0}", _graphId);
				return null;
			}
			return graphDefinition;
		}
	}

	public string GraphId => _graphId;

	public int DisplayPriority => GraphDefinition.Configuration.AchievementConfig.DisplayPriority;

	public string TitleLocalizationKey => GraphDefinition.Configuration.AchievementConfig.TitleLocKey;

	public string Title => _localizationProvider.GetLocalizedText(TitleLocalizationKey);

	public CollationMapping ExpansionCode => _expansionCode;

	public IList<IClientAchievementGroup> AchievementGroups => _achievementGroups;

	public DateTime EndTime => GraphDefinition.EndTime;

	public DateTime EndRevealTime => GraphDefinition.EndRevealTime;

	public event Action SetChanged;

	private AchievementSet()
	{
		_localizationProvider = Pantry.Get<IClientLocProvider>();
		_campaignGraphManager = Pantry.Get<CampaignGraphManager>();
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
	}

	public AchievementSet(ClientGraphDefinition graphDefinition)
		: this()
	{
		_graphId = graphDefinition.Id;
		foreach (ClientAchievementGroupConfiguration group in graphDefinition.Configuration.AchievementConfig.Groups)
		{
			try
			{
				IClientAchievementGroup clientAchievementGroup = new AchievementGroup(GraphIdNodeId.From(_graphId, group.Id), this);
				_achievementGroups.Add(clientAchievementGroup);
				clientAchievementGroup.GroupChanged += OnAchievementGroupChanged;
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Failed to create Achievement Group {0} in the {1} set. See Below Error.\n{2}", group.Id, graphDefinition.Id, ex);
			}
		}
	}

	~AchievementSet()
	{
		foreach (IClientAchievementGroup achievementGroup in _achievementGroups)
		{
			achievementGroup.GroupChanged -= OnAchievementGroupChanged;
		}
	}

	private void OnAchievementGroupChanged()
	{
		this.SetChanged?.Invoke();
	}

	public Sprite GetAchievementSetIcon(AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.TextureName = GraphDefinition.Configuration.AchievementConfig.IconImage;
		return AssetLoader.GetObjectData<Sprite>(assetLookupSystem.TreeLoader.LoadTree<AchievementsSpritePayload>().GetPayload(assetLookupSystem.Blackboard).Reference.RelativePath);
	}
}
