using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IQualificationController
{
	void AddQualification(QualificationData qualification);

	void RemoveQualification(QualificationData qualification);

	bool TryGetRelatedMiniCDC(QualificationData qualification, out DuelScene_CDC relatedMiniCDC);
}
