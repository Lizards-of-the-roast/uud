using System.Collections.Generic;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ChoiceResultUXEvent_General : UXEvent
{
	public class ChoiceResultOutputter
	{
		public const string COMMA_SEPARATOR = ", ";

		private readonly IGreLocProvider _greLocManager;

		private readonly IClientLocProvider _clientLocManager;

		public ChoiceResultOutputter(IGreLocProvider greLocManager, IClientLocProvider clientLocManager)
		{
			_greLocManager = greLocManager;
			_clientLocManager = clientLocManager;
		}

		public static string GetParityKey(uint id)
		{
			if (id % 2 != 0)
			{
				return "DuelScene/ClientPrompt/ParityEmote_Odd";
			}
			return "DuelScene/ClientPrompt/ParityEmote_Even";
		}

		private static string GetTypeKindKey(uint id)
		{
			return $"Enum/TypeKind/TypeKind_{(TypeKind)id}";
		}

		public static string GetEnumNameForStaticList(StaticList domain)
		{
			return domain switch
			{
				StaticList.CardTypes => typeof(CardType).Name, 
				StaticList.SubTypes => typeof(SubType).Name, 
				StaticList.Colors => typeof(Wotc.Mtgo.Gre.External.Messaging.Color).Name, 
				_ => string.Empty, 
			};
		}

		public string GenerateOutputString(IReadOnlyList<uint> choiceValues, string choiceDomain)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (uint choiceValue in choiceValues)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				if (!(choiceDomain == "localization_id"))
				{
					if (choiceDomain == "number")
					{
						stringBuilder.Append(choiceValue);
					}
					else
					{
						Debug.LogWarning("Unhandled ChoiceDomain " + choiceDomain);
					}
				}
				else
				{
					stringBuilder.Append(choiceValue switch
					{
						879590u => _clientLocManager.GetLocalizedText("DuelScene/ClientPrompt/Expropriate_Vote_Time"), 
						879626u => _clientLocManager.GetLocalizedText("DuelScene/ClientPrompt/Expropriate_Vote_Money"), 
						_ => _greLocManager.GetLocalizedText(choiceValue), 
					});
				}
			}
			return stringBuilder.ToString();
		}

		public string GenerateOutputString(IReadOnlyList<uint> choiceValues, StaticList staticList)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (uint choiceValue in choiceValues)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				string enumNameForStaticList = GetEnumNameForStaticList(staticList);
				if (!string.IsNullOrEmpty(enumNameForStaticList))
				{
					stringBuilder.Append(_greLocManager.GetLocalizedTextForEnumValue(enumNameForStaticList, (int)choiceValue));
					continue;
				}
				switch (staticList)
				{
				case StaticList.CardNames:
				{
					string localizedText = _greLocManager.GetLocalizedText(choiceValue);
					stringBuilder.Append(localizedText);
					break;
				}
				case StaticList.Parities:
					stringBuilder.Append(_clientLocManager.GetLocalizedText(GetParityKey(choiceValue)));
					break;
				case StaticList.TypeKinds:
					stringBuilder.Append(_clientLocManager.GetLocalizedText(GetTypeKindKey(choiceValue)));
					break;
				default:
					Debug.LogWarning($"Unhandled StaticList {staticList}");
					break;
				}
			}
			return stringBuilder.ToString();
		}
	}

	public class ChoiceResultVfxPlayer
	{
		private readonly AssetLookupSystem _als;

		private readonly IVfxProvider _vfxProvider;

		private readonly ICardDataAdapter _affectorCard;

		public ChoiceResultVfxPlayer(ChoiceResultEvent cre, GameManager gameManager)
		{
			_als = gameManager.AssetLookupSystem;
			_vfxProvider = gameManager.VfxProvider;
			MtgCardInstance mtgCardInstance = gameManager.LatestGameState.GetCardById(cre.AffectorId) ?? gameManager.CurrentGameState.GetCardById(cre.AffectorId);
			if (mtgCardInstance != null)
			{
				_affectorCard = CardDataExtensions.CreateWithDatabase(mtgCardInstance, gameManager.CardDatabase);
			}
		}

		public void PlayChoiceResultVFX()
		{
			_als.Blackboard.Clear();
			_als.Blackboard.SetCardDataExtensive(_affectorCard);
			if (_als.TreeLoader.TryLoadTree(out AssetLookupTree<ChoiceResult> loadedTree))
			{
				ChoiceResult payload = loadedTree.GetPayload(_als.Blackboard);
				if (payload != null)
				{
					GameObject gameObject = _vfxProvider.PlayVFX(payload.VfxData, _affectorCard);
					if (payload.SfxData.AudioEvents.Count > 0 && (bool)gameObject)
					{
						AudioManager.PlayAudio(payload.SfxData.AudioEvents, gameObject);
					}
				}
			}
			_als.Blackboard.Clear();
		}
	}

	private const string CHOICE_DOMAIN_LOCALIZATION_ID = "localization_id";

	private const string CHOICE_DOMAIN_NUMBER = "number";

	private readonly ChoiceResultEvent _choiceResult;

	private readonly ChoiceResultOutputter _resultOutput;

	private readonly ChoiceResultVfxPlayer _resultVfxPlayer;

	private readonly IEntityDialogControllerProvider _dialogueProvider;

	public override bool IsBlocking => true;

	public ChoiceResultUXEvent_General(ChoiceResultEvent cre, GameManager gameManager)
	{
		_choiceResult = cre;
		_dialogueProvider = gameManager.Context.Get<IEntityDialogControllerProvider>() ?? NullEntityDialogControllerProvider.Default;
		_resultOutput = new ChoiceResultOutputter(gameManager.CardDatabase.GreLocProvider, gameManager.LocManager);
		_resultVfxPlayer = new ChoiceResultVfxPlayer(cre, gameManager);
	}

	public override bool CanExecute(List<UXEvent> currentlyRunningEvents)
	{
		return !currentlyRunningEvents.Exists((UXEvent x) => x.HasWeight);
	}

	public override void Execute()
	{
		string choiceResultText = GetChoiceResultText();
		if (!string.IsNullOrEmpty(choiceResultText) && _dialogueProvider.TryGetDialogControllerById(_choiceResult.AffectedId, out var dialogController))
		{
			dialogController.ShowPlayerChoice(choiceResultText);
		}
		_resultVfxPlayer?.PlayChoiceResultVFX();
		Complete();
	}

	private string GetChoiceResultText()
	{
		if (string.IsNullOrEmpty(_choiceResult.ChoiceDomainString) && _choiceResult.ChoiceDomainStaticListType == StaticList.None)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(_choiceResult.ChoiceDomainString))
		{
			return _resultOutput.GenerateOutputString(_choiceResult.ChoiceValues, _choiceResult.ChoiceDomainString);
		}
		return _resultOutput.GenerateOutputString(_choiceResult.ChoiceValues, _choiceResult.ChoiceDomainStaticListType);
	}
}
