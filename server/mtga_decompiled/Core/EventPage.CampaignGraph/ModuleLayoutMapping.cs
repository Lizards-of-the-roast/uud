using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace EventPage.CampaignGraph;

[Serializable]
public struct ModuleLayoutMapping
{
	public LayoutGroup LayoutGroup;

	public List<EventModule> EventModules;
}
