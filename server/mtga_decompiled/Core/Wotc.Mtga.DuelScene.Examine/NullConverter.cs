using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Examine;

public class NullConverter : IModelConverter
{
	public static readonly IModelConverter Default = new NullConverter();

	public ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None)
	{
		return null;
	}
}
