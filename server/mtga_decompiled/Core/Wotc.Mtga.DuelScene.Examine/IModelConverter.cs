using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Examine;

public interface IModelConverter
{
	ICardDataAdapter ConvertModel(ICardDataAdapter sourceModel, ExamineState examineState = ExamineState.None);
}
