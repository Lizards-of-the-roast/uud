using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

public class OptionalActionMessageWorkflow_Commander : WorkflowBase<OptionalActionMessageRequest>
{
	public OptionalActionMessageWorkflow_Commander(OptionalActionMessageRequest request)
		: base(request)
	{
	}

	protected override void ApplyInteractionInternal()
	{
		SetButtons();
	}

	protected override void SetButtons()
	{
		base.Buttons = new Buttons();
		ZoneType numberValue = (ZoneType)_request.Prompt.Parameters[1].NumberValue;
		bool flag = numberValue == ZoneType.Hand;
		ZoneType zoneType = (flag ? numberValue : ZoneType.Command);
		ZoneType zoneType2 = (flag ? ZoneType.Command : numberValue);
		OptionResponse primaryBtnResponse = ((!flag) ? OptionResponse.AllowYes : OptionResponse.CancelNo);
		OptionResponse secondaryBtnResponse = (flag ? OptionResponse.AllowYes : OptionResponse.CancelNo);
		base.Buttons.WorkflowButtons.Add(new PromptButtonData
		{
			ButtonText = getZoneText(zoneType),
			Style = ButtonStyle.StyleType.Main,
			Tag = ButtonTag.Primary,
			ButtonCallback = delegate
			{
				_request.SubmitResponse(primaryBtnResponse);
			}
		});
		base.Buttons.WorkflowButtons.Add(new PromptButtonData
		{
			ButtonText = getZoneText(zoneType2),
			Tag = ButtonTag.Secondary,
			Style = ButtonStyle.StyleType.Secondary,
			ButtonCallback = delegate
			{
				_request.SubmitResponse(secondaryBtnResponse);
			}
		});
		if (_request.AllowUndo)
		{
			base.Buttons.UndoData = new PromptButtonData
			{
				ButtonCallback = delegate
				{
					_request.Undo();
				}
			};
		}
		OnUpdateButtons(base.Buttons);
		static string getZoneText(ZoneType zoneType3)
		{
			return zoneType3 switch
			{
				ZoneType.Graveyard => "Enum/ZoneType/ZoneType_Graveyard", 
				ZoneType.Exile => "Enum/ZoneType/ZoneType_Exile", 
				ZoneType.Hand => "Enum/ZoneType/ZoneType_Hand", 
				ZoneType.Library => "Enum/ZoneType/ZoneType_Library", 
				ZoneType.Battlefield => "Enum/ZoneType/ZoneType_Battlefield", 
				ZoneType.Stack => "Enum/ZoneType/ZoneType_Stack", 
				ZoneType.Command => "Enum/ZoneType/ZoneType_Command", 
				_ => "Enum/ZoneType/ZoneType_Limbo", 
			};
		}
	}
}
