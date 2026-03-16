using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Parts.Textbox;

namespace Core.DuelScene.Cards;

public class CDCPart_TextBox_LTR_TheRing : CDCPart_TextBox_InteractableParent
{
	protected override void HandleUpdateInternal()
	{
		if (_overrideAbilities == null)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(87496u);
			_overrideAbilities = cardPrintingById.Abilities;
		}
		base.HandleUpdateInternal();
	}

	protected override void SetTextBoxStatus(ICardDataAdapter cardData)
	{
		if (cardData == null || cardData.Controller == null || cardData.Controller.GamewideCounts == null || !cardData.Controller.GamewideCounts.Any())
		{
			ResetAllTextBoxes();
			return;
		}
		foreach (GamewideCountData item in cardData.Controller?.GamewideCounts.Where((GamewideCountData x) => x.AbilityId == 290))
		{
			for (int num = 0; num < _textBoxes.Count; num++)
			{
				if (item.Count > num)
				{
					_textBoxes[num].SetInteraction(InteractableTextBox.TextBoxHighlight.Selectable, null);
				}
				else
				{
					_textBoxes[num].SetInteraction(InteractableTextBox.TextBoxHighlight.Default, null);
				}
			}
		}
	}

	private void ResetAllTextBoxes()
	{
		for (int i = 0; i < _textBoxes.Count; i++)
		{
			_textBoxes[i].SetInteraction(InteractableTextBox.TextBoxHighlight.Default, null);
		}
	}
}
