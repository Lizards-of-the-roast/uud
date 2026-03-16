namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class ActivationWordDetailWithThresholdForParent : ThresholdParentCalculator
{
	public string ActivationWord = string.Empty;

	public ActivationWordDetailWithThresholdForParent()
		: base(new ActivationWordDetailWithThreshold(), new ActivationWordAdditionalDetailCount())
	{
	}

	protected override void SetupChild()
	{
		if (_childThresholdCalculator is ActivationWordDetailWithThreshold activationWordDetailWithThreshold && _childCalculator is ActivationWordAdditionalDetailCount activationWordAdditionalDetailCount)
		{
			activationWordDetailWithThreshold.ActivationWord = ActivationWord;
			activationWordAdditionalDetailCount.ActivationWord = ActivationWord;
		}
	}
}
