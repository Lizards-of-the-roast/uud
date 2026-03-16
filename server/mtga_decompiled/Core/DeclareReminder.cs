using System.Collections.Generic;

public class DeclareReminder : NPEReminder
{
	public MTGALocalizedString SubmitText;

	public List<uint> KeyCreatures = new List<uint>();

	public DeclareReminder(MTGALocalizedString declareText, MTGALocalizedString submitText, float declareTooltipTime, float declareSparkyDispatchTime, params uint[] namedAttackers)
		: base(declareText, declareTooltipTime, declareSparkyDispatchTime)
	{
		SubmitText = submitText;
		foreach (uint item in namedAttackers)
		{
			KeyCreatures.Add(item);
		}
	}

	public override void ShowToolTip(NPEController npeController)
	{
		if (SparkySuggestedInstances.Count == 0)
		{
			npeController.ShowPrompt(SubmitText);
		}
		else
		{
			npeController.ShowPrompt(Text);
		}
	}

	public override void DispatchSparky(NPEController npeController, GameManager gameManager)
	{
		if (SparkySuggestedInstances.Count == 0)
		{
			npeController.DesiredSparkySpot = SparkySpots.PromptButton;
		}
		else
		{
			base.DispatchSparky(npeController, gameManager);
		}
	}
}
