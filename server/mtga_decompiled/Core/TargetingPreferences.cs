using System.Collections.Generic;
using GreClient.Rules;

public class TargetingPreferences
{
	public List<TargetCharacteristics> PreferredCharacteristics;

	public List<uint> PreferredTargets;

	public TargetingPreferences()
	{
		PreferredTargets = new List<uint>();
		PreferredCharacteristics = new List<TargetCharacteristics>();
	}

	public bool IsPreferredEntity(MtgEntity entity)
	{
		if (!(entity is MtgCardInstance card))
		{
			if (entity is MtgPlayer player)
			{
				return IsPreferredPlayer(player);
			}
			return false;
		}
		return IsPreferredCard(card);
	}

	public bool IsPreferredCard(MtgCardInstance card)
	{
		if (card == null)
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.IsUntapped) && card.IsTapped)
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.AIControls) && !card.Controller.IsLocalPlayer)
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.HumanControls) && card.Controller.IsLocalPlayer)
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.IsTheHumanPlayer) || PreferredCharacteristics.Contains(TargetCharacteristics.IsTheAIPlayer))
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.IsTapped) && !card.IsTapped)
		{
			return false;
		}
		return true;
	}

	public bool IsPreferredPlayer(MtgPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.IsTheHumanPlayer) && player.IsLocalPlayer)
		{
			return false;
		}
		if (PreferredCharacteristics.Contains(TargetCharacteristics.IsTheAIPlayer) && !player.IsLocalPlayer)
		{
			return false;
		}
		return true;
	}
}
