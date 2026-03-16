using AssetLookupTree;
using AssetLookupTree.Extractors.UI;
using AssetLookupTree.Payloads.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.GeneralUtilities;
using Wizards.Mtga.PlayBlade;
using Wotc.Mtga.Loc;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public bool IsActive = true;

	public TooltipData TooltipData;

	public TooltipContext tooltipContext;

	public TooltipProperties TooltipProperties;

	public bool OverridePropertiesWithALT;

	public LocalizedString LocString;

	[SerializeField]
	private bool _clickThrough;

	protected static TooltipSystem _tooltipSystem;

	public static void Inject(TooltipSystem tooltipSystem)
	{
		_tooltipSystem = tooltipSystem;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (IsActive)
		{
			ShowTooltip(eventData);
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (_clickThrough)
		{
			eventData.PassEventToNextClickableItem(base.gameObject);
		}
		PlayBladeEventTile componentInParent = base.gameObject.GetComponentInParent<PlayBladeEventTile>();
		if (componentInParent != null)
		{
			componentInParent.OnClicked();
		}
	}

	public void Awake()
	{
		if (OverridePropertiesWithALT)
		{
			TooltipProperties = null;
		}
	}

	protected void ShowTooltip(PointerEventData eventData)
	{
		if ((string)LocString != null && !string.IsNullOrEmpty(LocString.mTerm) && LocString.mTerm != "MainNav/General/Empty_String")
		{
			TooltipData.Text = LocString.mTerm;
		}
		if (!(_tooltipSystem != null))
		{
			return;
		}
		if (TooltipProperties == null)
		{
			AssetLookupSystem assetLookupSystem = _tooltipSystem.AssetLookupSystem;
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.TooltipContext = tooltipContext;
			if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TooltipPropertiesPayload> loadedTree))
			{
				TooltipPropertiesPayload payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null)
				{
					TooltipPropertiesObject objectData = AssetLoader.GetObjectData(payload.PropertyPath);
					if ((object)objectData != null)
					{
						TooltipProperties = objectData.Properties;
					}
				}
			}
		}
		_tooltipSystem.DisplayTooltip(base.gameObject, TooltipData, TooltipProperties);
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (!IsActive)
		{
			return;
		}
		GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
		if (!(gameObject != null) || !(gameObject.transform.parent != null) || !(gameObject.transform.parent.parent != null))
		{
			return;
		}
		GameObject gameObject2 = gameObject.transform.parent.parent.gameObject;
		if (gameObject2 != null)
		{
			PagesMetaCardView component = gameObject2.GetComponent<PagesMetaCardView>();
			if (component != null)
			{
				component.OnPointerEnter(eventData);
			}
		}
	}

	public string GetTooltipText()
	{
		if ((string)LocString != null && !string.IsNullOrEmpty(LocString.mTerm) && LocString.mTerm != "MainNav/General/Empty_String")
		{
			TooltipData.Text = LocString.mTerm;
		}
		return TooltipData.Text;
	}
}
