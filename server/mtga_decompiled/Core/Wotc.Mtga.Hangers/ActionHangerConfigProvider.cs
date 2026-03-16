using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ActionHangerConfigProvider : IActionHangerConfigProvider
{
	private readonly IClientLocProvider _locProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public ActionHangerConfigProvider(IClientLocProvider locProvider, AssetLookupSystem assetLookupSystem)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public HangerConfig GetHangerConfig(Action action)
	{
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<InfoHanger> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.GreAction = action;
			InfoHanger payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				string header = (string.IsNullOrEmpty(payload.HeaderLocKey.Key) ? string.Empty : _locProvider.GetLocalizedText(payload.HeaderLocKey.Key));
				string details = (string.IsNullOrEmpty(payload.BodyLocKey.Key) ? string.Empty : _locProvider.GetLocalizedText(payload.BodyLocKey.Key));
				string addendum = (string.IsNullOrEmpty(payload.AddendumLocKey.Key) ? string.Empty : _locProvider.GetLocalizedText(payload.AddendumLocKey.Key));
				string spritePath = (string.IsNullOrEmpty(payload.BadgeRef.RelativePath) ? string.Empty : payload.BadgeRef.RelativePath);
				return new HangerConfig(header, details, addendum, spritePath);
			}
		}
		return default(HangerConfig);
	}
}
