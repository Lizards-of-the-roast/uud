using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.DuelScene;

namespace AssetLookupTree.Payloads.UI.DuelScene;

public class DuelSceneButtonPrefabs : IPayload
{
	public readonly AltAssetReference<Transform> ButtonsLayoutRef = new AltAssetReference<Transform>();

	public readonly AltAssetReference<ButtonPhaseLadder> ButtonPhaseLadderRef = new AltAssetReference<ButtonPhaseLadder>();

	public readonly AltAssetReference<StyledButton> PrimaryButtonRef = new AltAssetReference<StyledButton>();

	public readonly AltAssetReference<StyledButton> SecondaryButtonRef = new AltAssetReference<StyledButton>();

	public readonly AltAssetReference<ButtonPhaseContext> ButtonPhaseContextRef = new AltAssetReference<ButtonPhaseContext>();

	public readonly AltAssetReference<EndTurnButton> ButtonPhaseToEndRef = new AltAssetReference<EndTurnButton>();

	public readonly AltAssetReference<FullControl> FullControlRef = new AltAssetReference<FullControl>();

	public readonly AltAssetReference<KeyboardToggleButton> UndoButtonRef = new AltAssetReference<KeyboardToggleButton>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ButtonsLayoutRef.RelativePath;
		yield return ButtonPhaseLadderRef.RelativePath;
		yield return PrimaryButtonRef.RelativePath;
		yield return SecondaryButtonRef.RelativePath;
		yield return ButtonPhaseContextRef.RelativePath;
		yield return ButtonPhaseToEndRef.RelativePath;
		yield return FullControlRef.RelativePath;
		yield return UndoButtonRef.RelativePath;
	}
}
