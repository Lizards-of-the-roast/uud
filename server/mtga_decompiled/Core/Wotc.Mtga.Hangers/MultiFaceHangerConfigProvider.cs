using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class MultiFaceHangerConfigProvider : IHangerConfigProvider
{
	private IClientLocProvider _clientLocManager;

	private IGreLocProvider _greLocManager;

	private AssetLookupSystem _alt;

	private IFaceInfoGenerator _faceInfoGeneratorExamine;

	public IFaceInfoGenerator FaceInfoGenerator { private get; set; }

	public BASE_CDC CardView { private get; set; }

	public MultiFaceHangerConfigProvider(IClientLocProvider clientLocManager, IGreLocProvider greLocManager, AssetLookupSystem alt, ICardDatabaseAdapter cardDatabase, DeckFormat currentEventFormat, IObjectPool genericPool)
	{
		_clientLocManager = clientLocManager;
		_greLocManager = greLocManager;
		_alt = alt;
		_faceInfoGeneratorExamine = FaceInfoGeneratorFactory.DuelScene.ExamineGenerator(cardDatabase, alt, currentEventFormat, genericPool);
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		bool inDuelScene = false;
		CardHolderType cardHolderType = CardHolderType.None;
		if (CardView is DuelScene_CDC duelScene_CDC)
		{
			inDuelScene = true;
			cardHolderType = duelScene_CDC.CurrentCardHolder?.CardHolderType ?? CardHolderType.None;
		}
		uint hoverFaceHangerCount = FaceHangerCount(model, FaceInfoGenerator);
		uint examineFaceHangerCount = FaceHangerCount(model, _faceInfoGeneratorExamine);
		_alt.Blackboard.Clear();
		_alt.Blackboard.InDuelScene = inDuelScene;
		_alt.Blackboard.InWrapper = PAPA.SceneLoading.CurrentScene == PAPA.MdnScene.Wrapper;
		_alt.Blackboard.CanCraft = SceneLoader.GetSceneLoader()?.CurrentNavContent is WrapperDeckBuilder wrapperDeckBuilder && wrapperDeckBuilder.CanCraft;
		_alt.Blackboard.CardHolderType = cardHolderType;
		_alt.Blackboard.HoverFaceHangerCount = hoverFaceHangerCount;
		_alt.Blackboard.ExamineFaceHangerCount = examineFaceHangerCount;
		InfoHanger payload = _alt.TreeLoader.LoadTree<InfoHanger>().GetPayload(_alt.Blackboard);
		if (payload != null)
		{
			string localizedText = _greLocManager.GetLocalizedText(model.TitleId);
			string header = payload.HeaderLocKey.GetText(_clientLocManager, _greLocManager);
			string text = payload.BodyLocKey.GetText(_clientLocManager, _greLocManager, new Dictionary<string, string> { { "cardName", localizedText } });
			string text2 = (string.IsNullOrEmpty(payload.AddendumLocKey.Key) ? string.Empty : ((string)payload.AddendumLocKey.GetText(_clientLocManager, _greLocManager)));
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + "\n" + text2;
			}
			yield return new HangerConfig(header, text, null, payload.BadgeRef.RelativePath);
		}
	}

	private uint FaceHangerCount(ICardDataAdapter model, IFaceInfoGenerator generator)
	{
		if (model == null || generator == null || CardView == null)
		{
			return 0u;
		}
		uint num = 0u;
		foreach (FaceHanger.FaceCardInfo item in generator.GenerateFaceCardInfo(CardView.Model, model))
		{
			_ = item;
			num++;
		}
		return num;
	}
}
