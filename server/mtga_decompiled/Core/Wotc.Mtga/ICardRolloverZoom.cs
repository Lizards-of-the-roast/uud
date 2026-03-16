using System;
using GreClient.CardData;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga;

public interface ICardRolloverZoom
{
	Action<ICardDataAdapter> OnRolloverStart { get; set; }

	Action<Meta_CDC> OnRollover { get; set; }

	Action<Meta_CDC> OnRolloff { get; set; }

	ICardDataAdapter LastRolloverModel { get; }

	bool IsActive { get; set; }

	void Destroy();

	void Initialize(CardViewBuilder cardViewBuilder, CardDatabase cardDatabase, IClientLocProvider locManager, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, KeyboardManager keyboardManager, DeckFormat currentEventFormat);

	bool CardRolledOver(ICardDataAdapter model, Bounds cardColliderBounds, HangerSituation hangerSituation = default(HangerSituation), Vector2 offset = default(Vector2));

	void CardRolledOff(ICardDataAdapter model, bool alwaysRollOff = false);

	void CardPointerDown(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null, HangerSituation hangerSituation = default(HangerSituation));

	void CardPointerUp(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null);

	bool CardScrolled(Vector2 scrollDelta);

	void Close();

	bool HandleKeyDown(KeyCode curr, Modifiers mods);

	void Cleanup();
}
