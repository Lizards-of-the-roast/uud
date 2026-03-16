using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public abstract class MultistepEffectUXEventBase : UXEvent
{
	protected ICardDataAdapter _affectorData;

	protected readonly GameManager _gameManager;

	protected readonly IVfxProvider _vfxProvider;

	private readonly CardHolderReference<StackCardHolder> _stack;

	protected readonly List<VfxData> _vfxs = new List<VfxData>(10);

	protected SfxData _sfxData;

	public MtgCardInstance Affector { get; private set; }

	public MtgPlayer Affected { get; private set; }

	public AbilitySubCategory AbilityCategory { get; private set; }

	public MultistepEffectUXEventBase(MtgPlayer affected, MtgCardInstance affector, AbilitySubCategory abilityCategory, GameManager gameManager)
	{
		Affector = affector;
		Affected = affected;
		AbilityCategory = abilityCategory;
		_gameManager = gameManager;
		_vfxProvider = gameManager.VfxProvider;
		_affectorData = CardDataExtensions.CreateWithDatabase(Affector, _gameManager.CardDatabase);
		_stack = CardHolderReference<StackCardHolder>.Stack(gameManager.CardHolderManager);
	}

	public override void Execute()
	{
		if (_vfxs != null && _vfxs.Count > 0)
		{
			foreach (VfxData vfx in _vfxs)
			{
				_vfxProvider.PlayVFX(vfx, _affectorData);
			}
		}
		if (_sfxData != null)
		{
			AudioManager.PlayAudio(_sfxData.AudioEvents, _stack.Get().gameObject);
		}
		Complete();
	}

	protected AssetLookupSystem GetALTWithParams(ICardDataAdapter affector)
	{
		AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(affector);
		assetLookupSystem.Blackboard.Ability = GetAffectorAbility(affector);
		assetLookupSystem.Blackboard.Player = Affected;
		return assetLookupSystem;
	}

	private AbilityPrintingData GetAffectorAbility(ICardDataAdapter affector)
	{
		AbilityPrintingData abilityPrintingData = null;
		foreach (AbilityPrintingData ability in affector.Abilities)
		{
			if (ability.SubCategory == AbilityCategory)
			{
				abilityPrintingData = ability;
			}
		}
		if (abilityPrintingData == null)
		{
			AbilitySubCategory abilityCategory = AbilityCategory;
			abilityPrintingData = new AbilityPrintingData(new AbilityPrintingRecord(0u, 0u, null, 0u, 0u, null, Wotc.Mtgo.Gre.External.Messaging.AbilityCategory.None, abilityCategory), _gameManager.CardDatabase.AbilityDataProvider);
		}
		return abilityPrintingData;
	}

	protected override void Cleanup()
	{
		_stack.ClearCache();
		base.Cleanup();
	}
}
