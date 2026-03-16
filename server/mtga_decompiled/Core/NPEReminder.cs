using System;
using System.Collections.Generic;

public abstract class NPEReminder
{
	public MTGALocalizedString Text;

	public float TimeToWaitForToolTip;

	public float TimeToWaitForSparkyDispatch;

	public DateTime TimeOfActivation;

	public List<uint> SparkySuggestedInstances = new List<uint>();

	public NPEReminder(MTGALocalizedString text, float tooltipTime, float sparkyDispatchTime)
	{
		Text = text;
		TimeToWaitForToolTip = tooltipTime;
		TimeToWaitForSparkyDispatch = sparkyDispatchTime;
	}

	public virtual void ShowToolTip(NPEController npeController)
	{
		npeController.ShowPrompt(Text);
	}

	public virtual void DispatchSparky(NPEController npeController, GameManager gameManager)
	{
		int index = NPEDirector.RANDOM.Next(SparkySuggestedInstances.Count);
		if (gameManager.ViewManager.TryGetCardView(SparkySuggestedInstances[index], out var cardView))
		{
			npeController.SparkyTargetCardView = cardView;
			npeController.DesiredSparkySpot = SparkySpots.ACard;
		}
		else
		{
			npeController.ReminderInFinalForm();
		}
	}
}
