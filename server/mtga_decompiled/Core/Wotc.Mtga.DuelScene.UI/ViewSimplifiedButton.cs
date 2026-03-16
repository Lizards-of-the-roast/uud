using AssetLookupTree;
using AssetLookupTree.Payloads.CardHolder;
using GreClient.CardData;
using Wotc.Mtga.DuelScene.Examine;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.UI;

public class ViewSimplifiedButton : ViewPrintingButton
{
	public override ExamineState DefaultState { get; protected set; } = ExamineState.Styled;

	public override void Init(IClientLocProvider clientLocProvider, AssetLookupSystem assetLookupSystem, BASE_CDC clonedCardView)
	{
		base.Init(clientLocProvider, assetLookupSystem, clonedCardView);
	}

	public override ExamineState FindNextExamineState()
	{
		if (!IsStyledCard() && CurrentState == ExamineState.None)
		{
			return ExamineState.None;
		}
		ExamineState currentState = CurrentState;
		if (currentState == ExamineState.None || currentState == ExamineState.Styled)
		{
			CurrentState = ExamineState.Unstyled;
		}
		else
		{
			CurrentState = ExamineState.Styled;
		}
		return CurrentState;
	}

	public override void SetButtonText()
	{
		SetText(_locProvider.GetLocalizedText("DuelScene/ClientPrompt/ViewSimplified"));
	}

	public override bool ShouldShowButton(ExamineState currentState, ICardDataAdapter sourceModel, ICardDataAdapter examineModel)
	{
		if (sourceModel == null || examineModel == null)
		{
			return false;
		}
		if (sourceModel.Instance != null && (sourceModel.Instance.FaceDownState.IsFaceDown || sourceModel.Instance.FaceDownState.IsCopiedFaceDown))
		{
			return false;
		}
		if (CurrentState == ExamineState.Unstyled)
		{
			return true;
		}
		bool flag = !string.IsNullOrEmpty(examineModel.Instance?.SkinCode);
		bool flag2 = !string.IsNullOrEmpty(sourceModel.Instance?.SkinCode);
		if (flag && !flag2)
		{
			return !sourceModel.Instance.IsForceSimplified;
		}
		if (!flag && flag2)
		{
			return true;
		}
		if (examineModel.Printing != null && sourceModel.Printing != null)
		{
			if (!sourceModel.Printing.RawFrameDetails.Equals(string.Empty) && examineModel.Printing.RawFrameDetails.Equals(string.Empty))
			{
				return true;
			}
			if (sourceModel.Printing.AdditionalFrameDetails.Count > 0 && examineModel.Printing.AdditionalFrameDetails.Count == 0)
			{
				return true;
			}
			if (sourceModel.Printing.ExtraFrameDetails.Count > 0 && examineModel.Printing.ExtraFrameDetails.Count == 0)
			{
				return true;
			}
		}
		return IsStyledCard();
	}

	public bool IsStyledCard()
	{
		if (_sourceModel == null)
		{
			return false;
		}
		if (_assetLookupSystem.TreeLoader != null && _assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Examine_ViewSimplifiedButtonOverride> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(_sourceModel);
			_assetLookupSystem.Blackboard.CardHolderType = _sourceCardHolder;
			Examine_ViewSimplifiedButtonOverride payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_assetLookupSystem.Blackboard.Clear();
				return payload.ShowViewSimplifiedToggle;
			}
			if (_sourceModel.TitleId != ClonedCardView.VisualModel.TitleId)
			{
				_assetLookupSystem.Blackboard.SetCardDataExtensive(ClonedCardView.VisualModel);
				_assetLookupSystem.Blackboard.CardHolderType = _sourceCardHolder;
				payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					_assetLookupSystem.Blackboard.Clear();
					return payload.ShowViewSimplifiedToggle;
				}
			}
		}
		if (_sourceModel.Instance != null)
		{
			return !string.IsNullOrEmpty(_sourceModel.Instance.SkinCode);
		}
		return false;
	}
}
