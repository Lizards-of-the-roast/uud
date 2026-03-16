using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Payloads.Event;
using Core.Meta.MainNavigation.EventPageV2;
using Core.Meta.Tokens;
using Core.Shared.Code.ClientModels;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.NavBar;

public class NavBarTokenView : MonoBehaviour
{
	[SerializeField]
	private TooltipTrigger _tokenTooltip;

	[SerializeField]
	private ContainerProxy _tokenViewContainer;

	private IClientLocProvider _localizationManager;

	private AssetLookupSystem _assetLookupSystem;

	public void Init(IClientLocProvider locMan, AssetLookupSystem assetLookupSystem)
	{
		_localizationManager = locMan;
		_assetLookupSystem = assetLookupSystem;
		_tokenTooltip.LocString = new LocalizedString("", locMan);
	}

	public void UpdateTokensTooltip(List<Client_CustomTokenDefinitionWithQty> eventTokens)
	{
		List<Client_CustomTokenDefinitionWithQty> eventTokens2 = eventTokens.OrderByDescending((Client_CustomTokenDefinitionWithQty x) => x.DisplayPriority).ToList();
		bool active = ShouldShowToken(eventTokens2);
		AltAssetReference<TokenView> instance = PrefabRefForTokenView(eventTokens2, _assetLookupSystem);
		_tokenViewContainer.SetInstance(instance);
		_tokenTooltip.gameObject.UpdateActive(active);
		_tokenTooltip.TooltipData.Text = TooltipForTokens(_localizationManager, eventTokens2);
	}

	private static AltAssetReference<TokenView> PrefabRefForTokenView(IReadOnlyCollection<Client_CustomTokenDefinitionWithQty> eventTokens, AssetLookupSystem assetLookupSystem)
	{
		if (!ShouldShowToken(eventTokens))
		{
			return null;
		}
		foreach (Client_CustomTokenDefinitionWithQty eventToken in eventTokens)
		{
			AltAssetReference<TokenView> altAssetReference = TokenViewUtilities.TokenRefForId<NavBarTokenPayload, TokenView>(assetLookupSystem, eventToken?.PrefabName);
			if (altAssetReference != null)
			{
				return altAssetReference;
			}
		}
		return null;
	}

	private static bool ShouldShowToken(IEnumerable<Client_CustomTokenDefinitionWithQty> eventTokens)
	{
		return (eventTokens?.Sum((Client_CustomTokenDefinitionWithQty token) => token.Quantity) ?? 0) > 0;
	}

	private static string TooltipForTokens(IClientLocProvider locManager, List<Client_CustomTokenDefinitionWithQty> eventTokens)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < eventTokens.Count; i++)
		{
			Client_CustomTokenDefinitionWithQty client_CustomTokenDefinitionWithQty = eventTokens[i];
			if (client_CustomTokenDefinitionWithQty.Quantity > 0)
			{
				stringBuilder.AppendLine(locManager.GetLocalizedText(TokenViewUtilities.GetTokenLocalizationKey(client_CustomTokenDefinitionWithQty.TokenId, client_CustomTokenDefinitionWithQty.HeaderLocKey, client_CustomTokenDefinitionWithQty.Quantity), ("quantity", client_CustomTokenDefinitionWithQty.Quantity.ToString("N0"))));
				if (!string.IsNullOrEmpty(client_CustomTokenDefinitionWithQty.DescriptionLocKey))
				{
					stringBuilder.AppendLine(locManager.GetLocalizedText(client_CustomTokenDefinitionWithQty.DescriptionLocKey));
				}
				if (i < eventTokens.Count - 1)
				{
					stringBuilder.Append("<style=\"TooltipSpacer\"></style>");
				}
			}
		}
		return stringBuilder.ToString();
	}
}
