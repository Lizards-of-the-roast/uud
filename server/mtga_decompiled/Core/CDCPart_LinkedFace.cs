using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Extensions;

public class CDCPart_LinkedFace : CDCPart
{
	[SerializeField]
	private int _linkedFaceIndex;

	[SerializeField]
	private bool _ignoreInstance;

	public BASE_CDC LinkedFaceCDC { get; private set; }

	protected override void HandleUpdateInternal()
	{
		ICardDataAdapter cardDataAdapter = _cachedModel.GetLinkedFaceAtIndex(_linkedFaceIndex, _ignoreInstance, _cardDatabase.CardDataProvider) ?? CardDataExtensions.CreateBlank();
		EnsureCDC();
		if (LinkedFaceCDC.Model != cardDataAdapter)
		{
			LinkedFaceCDC.SetModel(cardDataAdapter);
		}
		else
		{
			LinkedFaceCDC.IsDirty = true;
		}
		LinkedFaceCDC.SetDimmedState(_cachedViewMetadata.IsDimmed);
		LinkedFaceCDC.UpdateVisibility(!_cachedDestroyed);
		if (LinkedFaceCDC.PartsRoot.gameObject.activeSelf != LinkedFaceCDC.TargetVisibility)
		{
			LinkedFaceCDC.PartsRoot.gameObject.SetActive(LinkedFaceCDC.TargetVisibility);
			if (LinkedFaceCDC.TargetVisibility)
			{
				LinkedFaceCDC.ImmediateUpdate();
			}
		}
		else if (_cachedViewMetadata.IsMeta)
		{
			LinkedFaceCDC.ImmediateUpdate();
		}
	}

	protected override void HandleDestructionInternal()
	{
		EnsureCDC();
		if ((bool)LinkedFaceCDC)
		{
			LinkedFaceCDC.UpdateVisibility(!_cachedDestroyed);
		}
		base.HandleDestructionInternal();
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		if ((bool)LinkedFaceCDC)
		{
			_cardViewBuilder.DestroyCDC(LinkedFaceCDC);
			LinkedFaceCDC = null;
		}
	}

	private void EnsureCDC()
	{
		if (_cachedDestroyed)
		{
			return;
		}
		if (!LinkedFaceCDC)
		{
			if (_cachedViewMetadata.IsMeta)
			{
				LinkedFaceCDC = _cardViewBuilder.CreateMetaCdc(CardDataExtensions.CreateBlank());
			}
			else
			{
				LinkedFaceCDC = _cardViewBuilder.CreateDuelSceneCdc(CardDataExtensions.CreateBlank(), base.GetCurrentGameState, base.GetCurrentInteraction, base.VfxProvider, base.EntityNameProvider);
			}
		}
		LinkedFaceCDC.Root.SetParent(base.transform);
		LinkedFaceCDC.Root.ZeroOut();
		LinkedFaceCDC.CollisionRoot.gameObject.SetActive(value: false);
		LinkedFaceCDC.gameObject.SetLayer(base.gameObject.layer);
		LinkedFaceCDC.HolderTypeOverride = _cachedCardHolderType;
	}
}
