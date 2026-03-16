namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class ActivationWordAdditionalDetailCountForParent : ParentCalculator
{
	public string ActivationWord = string.Empty;

	public ActivationWordAdditionalDetailCountForParent()
		: base(new ActivationWordAdditionalDetailCount())
	{
	}

	protected override void SetupChild()
	{
		if (_childCalculator is ActivationWordAdditionalDetailCount activationWordAdditionalDetailCount)
		{
			activationWordAdditionalDetailCount.ActivationWord = ActivationWord;
		}
	}
}
