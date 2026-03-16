using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Interactions;

public class PassButtonDataProvider : IButtonDataProvider
{
	private readonly IBlackboard _blackboard;

	private readonly AssetLookupTree<ButtonTextPayload> _buttonTextTree;

	private readonly AssetLookupTree<ButtonSfxPayload> _buttonSfxTree;

	private readonly AssetLookupTree<ButtonStylePayload> _buttonStyleTree;

	public PassButtonDataProvider(AssetLookupSystem assetLookupSystem)
	{
		_blackboard = assetLookupSystem.Blackboard ?? new Blackboard();
		_buttonTextTree = assetLookupSystem.TreeLoader.LoadTree<ButtonTextPayload>();
		_buttonSfxTree = assetLookupSystem.TreeLoader.LoadTree<ButtonSfxPayload>();
		_buttonStyleTree = assetLookupSystem.TreeLoader.LoadTree<ButtonStylePayload>();
	}

	public string GetLocKey()
	{
		ButtonTextPayload payload = _buttonTextTree.GetPayload(_blackboard);
		if (payload == null)
		{
			return string.Empty;
		}
		return payload.LocKey.Key;
	}

	public ButtonStyle.StyleType GetStyle()
	{
		return _buttonStyleTree.GetPayload(_blackboard)?.Style ?? ButtonStyle.StyleType.None;
	}

	public string GetSfx()
	{
		ButtonSfxPayload payload = _buttonSfxTree.GetPayload(_blackboard);
		if (payload == null)
		{
			return string.Empty;
		}
		return GetAudioEventName(payload);
	}

	private static string GetAudioEventName(ButtonSfxPayload payload)
	{
		if (payload.SfxData.AudioEvents.Count == 0)
		{
			return string.Empty;
		}
		return payload.SfxData.AudioEvents[0].WwiseEventName;
	}
}
