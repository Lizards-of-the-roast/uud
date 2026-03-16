namespace Core.Meta.MainNavigation.Store;

public class StoreScreenWrapperCompassGuide : WrapperCompassGuide
{
	public StoreTabType Context { get; }

	public string ItemId { get; }

	public string ExpansionCode { get; }

	public StoreScreenWrapperCompassGuide(StoreTabType context)
	{
		Context = context;
	}

	public StoreScreenWrapperCompassGuide(string itemId, StoreTabType fallbackContext)
	{
		Context = fallbackContext;
		ItemId = itemId;
	}

	public StoreScreenWrapperCompassGuide(string expansionCode)
	{
		Context = StoreTabType.Packs;
		ExpansionCode = expansionCode;
	}
}
