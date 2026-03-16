using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Avatar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Wrapper.Draft;

public class TableDraftSeatView : MonoBehaviour, ITableDraftSeatView
{
	[SerializeField]
	private TextMeshProUGUI _displayNameText;

	[SerializeField]
	private TextMeshProUGUI _statusText;

	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private Transform _boosterViewQueueParent;

	protected AssetLookupSystem _assetLookupSystem;

	private AssetLoader.AssetTracker<Sprite> _avatarImageSpriteTracker;

	private Dictionary<string, string> _locParameters = new Dictionary<string, string>
	{
		{ "pack", "0" },
		{ "pick", "0" }
	};

	public List<IDraftBoosterView> DraftBoosterViews { get; private set; } = new List<IDraftBoosterView>();

	public void Initialize(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	public void SetBustVisualData(BustVisualData visualData)
	{
		_displayNameText.text = visualData.DisplayName;
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CosmeticAvatarId = visualData.AvatarId;
		BustPayload payload = _assetLookupSystem.TreeLoader.LoadTree<BustPayload>().GetPayload(_assetLookupSystem.Blackboard);
		if (payload != null)
		{
			if (_avatarImageSpriteTracker == null)
			{
				_avatarImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("TableDraftAvatarImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarImageSpriteTracker, payload.Reference.RelativePath);
		}
	}

	public void AddDraftBoosterViews(IDraftBoosterView[] draftBoosterViews)
	{
		for (int i = 0; i < draftBoosterViews.Length; i++)
		{
			DraftBoosterView draftBoosterView = draftBoosterViews[i] as DraftBoosterView;
			draftBoosterView.transform.parent = _boosterViewQueueParent;
			draftBoosterView.transform.ZeroOut();
			DraftBoosterViews.Add(draftBoosterView);
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_avatarImage, _avatarImageSpriteTracker);
	}
}
