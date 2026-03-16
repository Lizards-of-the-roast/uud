using System.Collections.Generic;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Events;

public static class ClientGraphDefinitionExtensions
{
	public static List<ClientNodeDefinition> GetObjectiveBubbleNodes(this ClientGraphDefinition graphDefinition)
	{
		List<ClientNodeDefinition> list = new List<ClientNodeDefinition>();
		foreach (ClientNodeDefinition value in graphDefinition.Nodes.Values)
		{
			if (value.UXInfo?.ObjectiveBubbleUXInfo != null)
			{
				list.Add(value);
			}
		}
		return list;
	}
}
