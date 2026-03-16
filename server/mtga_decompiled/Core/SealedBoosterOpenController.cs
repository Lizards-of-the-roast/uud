using AssetLookupTree;
using Core.Meta.MainNavigation.Store;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

public class SealedBoosterOpenController : NavContentController
{
	private ContentControllerRewards _rewardsPanel;

	private ICardRolloverZoom _zoomHandler;

	private DeckBuilderContext _context;

	private int _gemsAdded;

	private SealedBoosterOpenAnimation _openInstance;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CosmeticsProvider _cosmetics;

	private AssetLookupSystem _assetLookupSystem;

	private ISetMetadataProvider _setMetadataProvider;

	private InventoryManager _inventoryManager;

	public override NavContentType NavContentType => NavContentType.SealedBoosterOpen;

	public void Init(ICardRolloverZoom zoomView, ContentControllerRewards rewardsController, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CosmeticsProvider cosmetics, AssetLookupSystem assetLookupSystem, ISetMetadataProvider setMetadataProvider, InventoryManager inventoryManager)
	{
		_zoomHandler = zoomView;
		_rewardsPanel = rewardsController;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cosmetics = cosmetics;
		_assetLookupSystem = assetLookupSystem;
		_setMetadataProvider = setMetadataProvider;
		_inventoryManager = inventoryManager;
	}

	public void SetContext(DeckBuilderContext context, int gemsAdded)
	{
		_context = context;
		_gemsAdded = gemsAdded;
	}

	public override void Activate(bool active)
	{
		if (active)
		{
			_openInstance = GetComponentInChildren<SealedBoosterOpenAnimation>(includeInactive: true);
			_openInstance.Init(_context, _gemsAdded, _rewardsPanel, _zoomHandler, _cardDatabase, _cardViewBuilder, _cosmetics, _assetLookupSystem, _setMetadataProvider, _inventoryManager);
		}
		else
		{
			_openInstance.Cleanup();
		}
	}
}
