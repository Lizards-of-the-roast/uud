using UnityEngine;
using Wotc.Mtga.Hangers;

public struct HangerSituation
{
	public bool ShowFlavorText;

	public bool DelayActivation;

	public bool UseNPEHanger;

	public bool HideSummoningSickness;

	public bool HideTapped;

	public bool ShowOnlyTapped;

	public bool ShouldCycleFaces;

	public string[] BannedFormats;

	public HangerConfig? EmergencyTempBanHanger;

	public string[] RestrictedFormats;

	public AbilityHangerData[] ContextualHangers;

	public Bounds HoveredCardBounds;
}
