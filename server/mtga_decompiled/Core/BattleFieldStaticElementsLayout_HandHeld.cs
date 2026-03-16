using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.PlayerNameViews;
using Wotc.Mtga.DuelScene.UI;

public class BattleFieldStaticElementsLayout_HandHeld : BattleFieldStaticElementsLayout
{
	[Header("Handheld")]
	[SerializeField]
	protected RectTransform _localRevealContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _localRevealLayoutData;

	[SerializeField]
	protected RectTransform _opponentRevealContainer;

	protected BattleFieldStaticLayoutWorldSpaceElementData _opponentRevealLayoutData;

	[SerializeField]
	private bool _shouldUpdateUserNamesAnchorPosition = true;

	protected override void Awake()
	{
		base.Awake();
		_localRevealLayoutData = _localRevealContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
		_opponentRevealLayoutData = _opponentRevealContainer.GetComponent<BattleFieldStaticLayoutWorldSpaceElementData>();
	}

	public override IEnumerator UpdateCardHolderPosition(Transform cardHolder, CardHolderType cardHolderType, GREPlayerNum owner, bool refresh = false)
	{
		yield return new WaitForEndOfFrame();
		bool flag = owner == GREPlayerNum.LocalPlayer;
		BattleFieldStaticLayoutWorldSpaceElementData layoutElementData;
		RectTransform layoutRect;
		switch (cardHolderType)
		{
		case CardHolderType.Hand:
			layoutElementData = (flag ? _localHandLayoutData : _opponentHandLayoutData);
			layoutRect = (flag ? _localHandContainer : _opponentHandContainer);
			break;
		case CardHolderType.Graveyard:
			layoutElementData = (flag ? _localGraveyardLayoutData : _opponentGraveyardLayoutData);
			layoutRect = (flag ? _localGraveyardContainer : _opponentGraveyardContainer);
			break;
		case CardHolderType.Exile:
			layoutElementData = (flag ? _localExileLayoutData : _opponentExileLayoutData);
			layoutRect = (flag ? _localExileContainer : _opponentExileContainer);
			break;
		case CardHolderType.Library:
			layoutElementData = (flag ? _localLibraryLayoutData : _opponentLibraryLayoutData);
			layoutRect = (flag ? _localLibraryContainer : _opponentLibraryContainer);
			break;
		case CardHolderType.Command:
			layoutElementData = (flag ? _localCommandLayoutData : _opponentCommandLayoutData);
			layoutRect = (flag ? _localCommandContainer : _opponentCommandContainer);
			break;
		case CardHolderType.Stack:
			layoutElementData = _stackLayoutData;
			layoutRect = _stackContainer;
			break;
		case CardHolderType.Reveal:
			layoutElementData = (flag ? _localRevealLayoutData : _opponentRevealLayoutData);
			layoutRect = (flag ? _localRevealContainer : _opponentRevealContainer);
			break;
		default:
			yield break;
		}
		if (layoutElementData.Enabled)
		{
			yield return new WaitForEndOfFrame();
			if (refresh)
			{
				layoutElementData.RestoreTransformValues(cardHolder);
			}
			else
			{
				layoutElementData.StoreTransformValues(cardHolder);
			}
			UpdateWorldSpaceElementPosition(cardHolder, layoutRect, layoutElementData);
			CardHolderBase component = cardHolder.GetComponent<CardHolderBase>();
			if ((bool)component)
			{
				component.LayoutNow();
			}
		}
	}

	public override IEnumerator UpdateAvatarPosition(DuelScene_AvatarView avatar, GREPlayerNum owner, bool refresh = false)
	{
		yield return new WaitForEndOfFrame();
		bool num = owner == GREPlayerNum.LocalPlayer;
		BattleFieldStaticLayoutWorldSpaceElementData battleFieldStaticLayoutWorldSpaceElementData = (num ? _localAvatarLayoutData : _opponentAvatarLayoutData);
		if (refresh)
		{
			battleFieldStaticLayoutWorldSpaceElementData.RestoreTransformValues(avatar.transform);
		}
		else
		{
			battleFieldStaticLayoutWorldSpaceElementData.StoreTransformValues(avatar.transform);
		}
		UpdateWorldSpaceElementPosition(layoutRect: num ? _localAvatarContainer : _opponentAvatarContainer, targetTransform: avatar.transform, layoutElementData: battleFieldStaticLayoutWorldSpaceElementData);
		battleFieldStaticLayoutWorldSpaceElementData = (num ? _localFrameLayoutData : _opponentFrameLayoutData);
		if (refresh)
		{
			battleFieldStaticLayoutWorldSpaceElementData.RestoreTransformValues(avatar.FramePosition);
		}
		else
		{
			battleFieldStaticLayoutWorldSpaceElementData.StoreTransformValues(avatar.FramePosition);
		}
		UpdateWorldSpaceElementPosition(layoutRect: num ? _localFrameContainer : _opponentFrameContainer, targetTransform: avatar.FramePosition, layoutElementData: battleFieldStaticLayoutWorldSpaceElementData);
		battleFieldStaticLayoutWorldSpaceElementData = (num ? _localHpLayoutData : _opponentHpLayoutData);
		if (refresh)
		{
			battleFieldStaticLayoutWorldSpaceElementData.RestoreTransformValues(avatar.LifeDisplayPosition);
		}
		else
		{
			battleFieldStaticLayoutWorldSpaceElementData.StoreTransformValues(avatar.LifeDisplayPosition);
		}
		UpdateWorldSpaceElementPosition(layoutRect: num ? _localHpContainer : _opponentHpContainer, targetTransform: avatar.LifeDisplayPosition, layoutElementData: battleFieldStaticLayoutWorldSpaceElementData);
		battleFieldStaticLayoutWorldSpaceElementData = (num ? _localManaPoolLayoutData : _opponentManaPoolLayoutData);
		if (refresh)
		{
			battleFieldStaticLayoutWorldSpaceElementData.RestoreTransformValues(avatar.ManaPoolRoot);
		}
		else
		{
			battleFieldStaticLayoutWorldSpaceElementData.StoreTransformValues(avatar.ManaPoolRoot);
		}
		UpdateWorldSpaceElementPosition(layoutRect: num ? _localManaPoolContainer : _opponentManaPoolContainer, targetTransform: avatar.ManaPoolRoot, layoutElementData: battleFieldStaticLayoutWorldSpaceElementData);
		battleFieldStaticLayoutWorldSpaceElementData = (num ? _localCounterPoolLayoutData : _opponentCounterPoolLayoutData);
		if (refresh)
		{
			battleFieldStaticLayoutWorldSpaceElementData.RestoreTransformValues(avatar.CounterPoolRoot);
		}
		else
		{
			battleFieldStaticLayoutWorldSpaceElementData.StoreTransformValues(avatar.CounterPoolRoot);
		}
		UpdateWorldSpaceElementPosition(layoutRect: num ? _localCounterPoolContainer : _opponentCounterPoolContainer, targetTransform: avatar.CounterPoolRoot, layoutElementData: battleFieldStaticLayoutWorldSpaceElementData);
		battleFieldStaticLayoutWorldSpaceElementData = (num ? _localTurnFrameLayoutData : _opponentTurnFrameLayoutData);
		if (refresh)
		{
			battleFieldStaticLayoutWorldSpaceElementData.RestoreTransformValues(avatar.TurnFramePosition);
		}
		else
		{
			battleFieldStaticLayoutWorldSpaceElementData.StoreTransformValues(avatar.TurnFramePosition);
		}
		RectTransform layoutRect = (num ? _localTurnFrameContainer : _opponentTurnFrameContainer);
		UpdateWorldSpaceElementPosition(avatar.TurnFramePosition, layoutRect, battleFieldStaticLayoutWorldSpaceElementData);
	}

	public override IEnumerator UpdatePromptButtonsAnchorPosition(RectTransform promptButtonsRect)
	{
		yield return new WaitForEndOfFrame();
		UpdateScreenSpaceElementPosition(promptButtonsRect, _promptButtonsAnchorPoint);
	}

	public override IEnumerator UpdateUserNamesAnchorPosition(IReadOnlyList<PlayerNameViewData> playerNames)
	{
		if (!_shouldUpdateUserNamesAnchorPosition)
		{
			yield break;
		}
		yield return new WaitForEndOfFrame();
		foreach (PlayerNameViewData playerName in playerNames)
		{
			if (playerName.PlayerNum == GREPlayerNum.LocalPlayer)
			{
				PlayerName playerNameView = playerName.PlayerNameView;
				UpdateScreenSpaceElementPosition(playerNameView.TextRoot, _localUserNameAnchorPoint);
				UpdateScreenSpaceElementPosition(playerNameView.RankDisplay.GetComponent<RectTransform>(), _localRankAnchorPoint);
				UpdateScreenSpaceElementPosition(playerNameView.WinpipRoot.GetComponent<RectTransform>(), _localWinPipsAnchorPoint);
			}
			else
			{
				PlayerName playerNameView2 = playerName.PlayerNameView;
				UpdateScreenSpaceElementPosition(playerNameView2.TextRoot, _opponentUserNameAnchorPoint);
				UpdateScreenSpaceElementPosition(playerNameView2.RankDisplay.GetComponent<RectTransform>(), _opponentRankAnchorPoint);
				UpdateScreenSpaceElementPosition(playerNameView2.WinpipRoot.GetComponent<RectTransform>(), _opponentWinPipsAnchorPoint);
			}
		}
	}

	public override IEnumerator UpdateFullControlAnchorPosition(RectTransform fullControlRect)
	{
		yield return new WaitForEndOfFrame();
		UpdateScreenSpaceElementPosition(fullControlRect, _fullControlAnchorPoint);
	}

	public override IEnumerator UpdateTimerPositions(TimerManager timerManager)
	{
		yield return new WaitForEndOfFrame();
		UpdateScreenSpaceElementPosition(timerManager.LocalPlayerTimeoutDisplay.GetComponent<RectTransform>(), _localTimeoutDisplayAnchorPoint);
		UpdateScreenSpaceElementPosition(timerManager.LocalPlayerMatchTimer.GetComponent<RectTransform>(), _localMatchTimerAnchorPoint);
		UpdateScreenSpaceElementPosition(timerManager.OpponentPlayerTimeoutDisplay.GetComponent<RectTransform>(), _opponentTimeoutDisplayAnchorPoint);
		UpdateScreenSpaceElementPosition(timerManager.OpponentMatchTimer.GetComponent<RectTransform>(), _opponentMatchTimerAnchorPoint);
	}
}
