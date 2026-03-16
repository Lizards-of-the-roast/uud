using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.BrowserCardHeaders;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ModalBrowserCardHeaderProvider : IModalBrowserCardHeaderProvider
{
	public interface ISubProvider
	{
		bool TryGetHeaderData(ICardDataAdapter cardModel, AbilityPrintingData abilityData, Wotc.Mtgo.Gre.External.Messaging.Action action, out HeaderData headerData);
	}

	public readonly struct HeaderData : IEquatable<HeaderData>
	{
		public readonly string Header;

		public readonly string SubHeader;

		public readonly bool UseActionTypeHeader;

		public static HeaderData Null => new HeaderData(null, null, useActionTypeHeader: false);

		public HeaderData(string header, string subHeader)
			: this(header, subHeader, useActionTypeHeader: false)
		{
		}

		public HeaderData(string subHeader)
			: this(string.Empty, subHeader, useActionTypeHeader: false)
		{
		}

		public HeaderData(bool useActionTypeHeader, string subHeader)
			: this(string.Empty, subHeader, useActionTypeHeader)
		{
		}

		public HeaderData(string header, string subHeader, bool useActionTypeHeader)
		{
			Header = header;
			SubHeader = subHeader;
			UseActionTypeHeader = useActionTypeHeader;
		}

		public bool Equals(HeaderData other)
		{
			bool num = (string.IsNullOrEmpty(Header) ? string.IsNullOrEmpty(other.Header) : Header.Equals(other.Header));
			bool flag = (string.IsNullOrEmpty(SubHeader) ? string.IsNullOrEmpty(other.SubHeader) : SubHeader.Equals(other.SubHeader));
			if (num && flag)
			{
				return UseActionTypeHeader == other.UseActionTypeHeader;
			}
			return false;
		}

		public bool IsNull()
		{
			return Equals(Null);
		}
	}

	private readonly IAbilityDataProvider _abilityDataProvider;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly IStaticListLocProvider _staticListLocProvider;

	private readonly IReadOnlyList<ISubProvider> _subProvidersByPriority;

	public ModalBrowserCardHeaderProvider(ICardDatabaseAdapter cardDatabase, IStaticListLocProvider staticListLocProvider, IReadOnlyList<ISubProvider> subProviders)
	{
		_abilityDataProvider = cardDatabase.AbilityDataProvider;
		_clientLocProvider = cardDatabase.ClientLocProvider;
		_staticListLocProvider = staticListLocProvider ?? NullStaticListLocProvider.Default;
		_subProvidersByPriority = subProviders ?? Array.Empty<ISubProvider>();
	}

	public BrowserCardHeader.BrowserCardHeaderData GetBrowserCardInfo(ICardDataAdapter cardModel, Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		AbilityPrintingData abilityPrintingData = GetAbilityPrintingData(cardModel, action);
		foreach (ISubProvider item in _subProvidersByPriority)
		{
			if (item.TryGetHeaderData(cardModel, abilityPrintingData, action, out var headerData))
			{
				if (headerData.IsNull())
				{
					return null;
				}
				return new BrowserCardHeader.BrowserCardHeaderData(headerData.UseActionTypeHeader ? GetActionTypeHeader(action) : headerData.Header, headerData.SubHeader);
			}
		}
		return null;
	}

	private AbilityPrintingData GetAbilityPrintingData(ICardDataAdapter cardModel, Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		if (_abilityDataProvider.TryGetAbilityPrintingById(action.AbilityGrpId, out var ability))
		{
			return ability;
		}
		if (_abilityDataProvider.TryGetAbilityPrintingById(action.AlternativeGrpId, out var ability2))
		{
			return ability2;
		}
		if (cardModel.ObjectType == GameObjectType.Ability)
		{
			return _abilityDataProvider.GetAbilityPrintingById(cardModel.GrpId);
		}
		return null;
	}

	private string GetActionTypeHeader(Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		string headerLocKeyForAction = GetHeaderLocKeyForAction(action);
		(string, string)[] headerLocParamsForAction = GetHeaderLocParamsForAction(action);
		if (!string.IsNullOrEmpty(headerLocKeyForAction))
		{
			return _clientLocProvider.GetLocalizedText(headerLocKeyForAction, headerLocParamsForAction);
		}
		return string.Empty;
	}

	private string GetHeaderLocKeyForAction(Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		if (action.IsPlayAction())
		{
			if (!action.IsCategoricalAction())
			{
				return "DuelScene/Browsers/BrowserCardInfo_PlayWith";
			}
			return "DuelScene/Browsers/BrowserCardInfo_PlayAsTypeWith";
		}
		if (action.IsActivateAction())
		{
			if (!action.IsCategoricalAction())
			{
				return "DuelScene/Browsers/BrowserCardInfo_ActivateWith";
			}
			return "DuelScene/Browsers/BrowserCardInfo_ActivateAsTypeWith";
		}
		if (action.IsCastAction())
		{
			if (!action.IsCategoricalAction())
			{
				return "DuelScene/Browsers/BrowserCardInfo_CastWith";
			}
			return "DuelScene/Browsers/BrowserCardInfo_CastAsTypeWith";
		}
		return string.Empty;
	}

	private (string, string)[] GetHeaderLocParamsForAction(Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		if (!action.IsCategoricalAction())
		{
			return null;
		}
		string localizedText = _staticListLocProvider.GetLocalizedText((StaticList)action.SelectionType, (int)action.Selection);
		return new(string, string)[1] { ("type", localizedText) };
	}

	public static IReadOnlyList<ISubProvider> DefaultSubProviders(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IEntityNameProvider<MtgEntity> nameProvider, AssetLookupSystem assetLookupSystem)
	{
		return new List<ISubProvider>
		{
			new AbilityReferenceTypeHeaders(cardDatabase.ClientLocProvider, cardDatabase.GreLocProvider, assetLookupSystem),
			new ActivatedAbilityHeaders(cardDatabase.ClientLocProvider, cardDatabase.GreLocProvider, gameStateProvider, assetLookupSystem),
			new BaseIdAbilityHeaders(cardDatabase.AbilityTextProvider),
			new AbilityIdHeaders(cardDatabase.ClientLocProvider, cardDatabase.GreLocProvider, assetLookupSystem),
			new ActionSourceHeaders(cardDatabase.ClientLocProvider, cardDatabase.GreLocProvider, cardDatabase.CardDataProvider, gameStateProvider),
			new AssociatedEntityHeaders(gameStateProvider, nameProvider),
			new AlternateCostFallback(cardDatabase.ClientLocProvider, gameStateProvider),
			new LinkedFaceHeaders(cardDatabase.ClientLocProvider, cardDatabase.GreLocProvider, cardDatabase.AbilityDataProvider)
		};
	}
}
