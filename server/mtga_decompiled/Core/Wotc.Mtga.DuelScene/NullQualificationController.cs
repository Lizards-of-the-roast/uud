using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullQualificationController : IQualificationController
{
	public static readonly IQualificationController Default = new NullQualificationController();

	public void AddQualification(QualificationData qualification)
	{
	}

	public void RemoveQualification(QualificationData qualification)
	{
	}

	public bool TryGetRelatedMiniCDC(QualificationData qualification, out DuelScene_CDC relatedMiniCDC)
	{
		relatedMiniCDC = null;
		return false;
	}
}
