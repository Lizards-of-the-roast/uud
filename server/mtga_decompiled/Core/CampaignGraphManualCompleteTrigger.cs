using UnityEngine;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;

public class CampaignGraphManualCompleteTrigger : MonoBehaviour
{
	[Header("Campaign Graph Configuration")]
	[SerializeField]
	private string graphId = "";

	[SerializeField]
	private string nodeId = "";

	public void QueueNodeForStartupSequence()
	{
		if (string.IsNullOrEmpty(graphId) || string.IsNullOrEmpty(nodeId))
		{
			SimpleLog.LogError("GraphId and NodeId must be configured in the Unity editor before triggering");
			return;
		}
		CampaignGraphManager.AddPendingManualCompleteNode(new GraphIdNodeId
		{
			GraphId = graphId,
			NodeId = nodeId
		});
	}

	private void OnValidate()
	{
		if (string.IsNullOrEmpty(graphId))
		{
			SimpleLog.LogError("CampaignRewardTrigger: GraphId is required - please set it in the Unity inspector");
		}
		if (string.IsNullOrEmpty(nodeId))
		{
			SimpleLog.LogError("CampaignRewardTrigger: NodeId is required - please set it in the Unity inspector");
		}
	}
}
