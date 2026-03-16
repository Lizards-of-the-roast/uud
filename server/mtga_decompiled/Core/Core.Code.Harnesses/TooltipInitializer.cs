using System.Collections;
using Core.Code.AssetLookupTree.AssetLookup;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class TooltipInitializer : HarnessInitializer
{
	public TooltipSystem TooltipSystem;

	public static TooltipInitializer Instance;

	public override IEnumerator OnInitialize()
	{
		Instance = this;
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		TooltipSystem.Init(assetLookupManager.AssetLookupSystem, new NullBILogger(), EchoLocProvider.Default);
		yield break;
	}

	public override string Status()
	{
		return "Initializing tooltips.";
	}
}
