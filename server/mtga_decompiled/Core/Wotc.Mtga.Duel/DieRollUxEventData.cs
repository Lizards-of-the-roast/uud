using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.Duel;

[CreateAssetMenu(menuName = "ScriptableObject/UXEvent/Die Roll Data", fileName = "DieRollUxEventData")]
public class DieRollUxEventData : ScriptableObject, IDieRollUxEventData
{
	[SerializeField]
	private DiceView _diceViewPf;

	public IDiceView InstantiateDiceView(GREPlayerNum controller, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem)
	{
		DiceView diceView = Object.Instantiate(_diceViewPf);
		diceView.Initialize(controller, vfxProvider, assetLookupSystem);
		return diceView;
	}
}
