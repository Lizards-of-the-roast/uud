using Wotc.Mtga.Providers;

namespace Core.Shared.Code.Providers;

public struct StaticContentProviders
{
	public CosmeticsProvider CosmeticsProvider { get; set; }

	public ISurveyConfigProvider SurveyConfigProvider { get; set; }

	public ICardNicknamesProvider CardNicknamesProvider { get; set; }

	public IEmergencyCardBansProvider EmergencyBansProvider { get; set; }

	public IAchievementDataProvider AchievementsDataProvider { get; set; }

	public IQueueTipProvider QueueTipProvider { get; set; }
}
