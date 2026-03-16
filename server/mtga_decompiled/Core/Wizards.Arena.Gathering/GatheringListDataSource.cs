using Core.Meta.MainNavigation.SocialV2;
using UnityEngine;
using Wizards.GeneralUtilities.Object_Pooling_Scroll_Rect;
using Wizards.Mtga;

namespace Wizards.Arena.Gathering;

[RequireComponent(typeof(RecyclableScrollRect))]
public class GatheringListDataSource : MonoBehaviour, IRecyclableScrollRectDataSource
{
	private RecyclableScrollRect _recyclableScrollRect;

	private GatheringManager _gatheringManager;

	private void Awake()
	{
		_gatheringManager = Pantry.Get<GatheringManager>();
		_recyclableScrollRect = GetComponent<RecyclableScrollRect>();
		_recyclableScrollRect.Initialize(this);
	}

	public int GetItemCount()
	{
		return _gatheringManager.Gatherings.Count;
	}

	public void SetCell(ICell cell, int index)
	{
		_ = cell as GatheringListItem == null;
	}
}
