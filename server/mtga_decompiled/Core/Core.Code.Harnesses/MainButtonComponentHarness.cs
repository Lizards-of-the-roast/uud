using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Shared.Code.ClientModels;
using EventPage.Components;
using UnityEngine;
using Wizards.Arena.Enums.Store;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Events;
using Wotc.Mtga.Providers;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class MainButtonComponentHarness : Harness<MainButtonComponent, HarnessDropdownUI>
{
	[Serializable]
	public class MainButtonTestModel
	{
		public string Description;

		public PlayerEventModule currentModule;

		public List<CurrencyTestModel> currencies;

		public bool eventIsActive;

		public bool startingOrRejoiningDraft;

		public bool isLimited;

		public bool hasPrize;

		public List<EventEntryFeeInfo> entryFees;

		public override string ToString()
		{
			return Description;
		}
	}

	[Serializable]
	public class CurrencyTestModel
	{
		public string Id;

		public int quantity;
	}

	public class HarnessCustomTokenProvider : ICustomTokenProvider
	{
		private List<Client_CustomTokenDefinitionWithQty> _tokenDefinitions;

		public Dictionary<string, Client_CustomTokenDefinition> TokenDefinitions { get; set; }

		public HarnessCustomTokenProvider(List<Client_CustomTokenDefinitionWithQty> tokenDefinitions)
		{
			_tokenDefinitions = tokenDefinitions;
		}

		public List<Client_CustomTokenDefinitionWithQty> GetCustomTokensOfTypeWithQty(ClientTokenType tokenType)
		{
			return _tokenDefinitions;
		}

		public void UpdateQuantities(List<CurrencyTestModel> currencies)
		{
			foreach (CurrencyTestModel currency in currencies)
			{
				Client_CustomTokenDefinitionWithQty client_CustomTokenDefinitionWithQty = _tokenDefinitions.Find((Client_CustomTokenDefinitionWithQty tokenDef) => tokenDef.TokenId == currency.Id);
				if (client_CustomTokenDefinitionWithQty != null)
				{
					client_CustomTokenDefinitionWithQty.Quantity = currency.quantity;
				}
			}
		}

		public bool IsTokenOfType(string tokenId, ClientTokenType tokenType)
		{
			throw new NotImplementedException();
		}
	}

	[SerializeField]
	private MainButtonTestModel[] _viewModels;

	[SerializeField]
	private List<Client_CustomTokenDefinitionWithQty> _tokenDefinitions;

	private MainButtonComponentController _controller;

	private HarnessCustomTokenProvider _customTokenProvider;

	private ClientPlayerInventory _clientInventory;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
		ParseViewModels(_viewModels);
		_customTokenProvider = new HarnessCustomTokenProvider(_tokenDefinitions);
	}

	public static void ParseViewModels(IEnumerable<MainButtonTestModel> viewModels)
	{
		foreach (MainButtonTestModel viewModel in viewModels)
		{
			foreach (EventEntryFeeInfo entryFee in viewModel.entryFees)
			{
				if (entryFee.ReferenceId == "<null>")
				{
					entryFee.ReferenceId = null;
				}
			}
		}
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		_clientInventory = new ClientPlayerInventory();
		AssetLookupManager assetLookupManager = Pantry.Get<AssetLookupManager>();
		_controller = new MainButtonComponentController(_Instance, _clientInventory, assetLookupManager.AssetLookupSystem, _customTokenProvider, null, null);
		RectTransform component = _Instance.GetComponent<RectTransform>();
		HarnessCore.CenterView(component);
		component.sizeDelta = new Vector2(300f, 300f);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
	}

	private void OnValueChanged(int newValue)
	{
		MainButtonTestModel mainButtonTestModel = _viewModels[newValue];
		_clientInventory.CustomTokens = mainButtonTestModel.currencies.ToDictionary((CurrencyTestModel x) => x.Id, (CurrencyTestModel x) => x.quantity);
		_customTokenProvider.UpdateQuantities(mainButtonTestModel.currencies);
		_controller.Update(mainButtonTestModel.currentModule, mainButtonTestModel.eventIsActive, mainButtonTestModel.startingOrRejoiningDraft, mainButtonTestModel.isLimited, mainButtonTestModel.hasPrize, mainButtonTestModel.entryFees);
	}
}
