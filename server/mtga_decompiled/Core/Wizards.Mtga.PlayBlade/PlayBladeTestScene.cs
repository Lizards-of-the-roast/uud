using System;
using System.Collections.Generic;
using AssetLookupTree;
using Newtonsoft.Json;
using UnityEngine;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Rank;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wizards.Mtga.PlayBlade;

public class PlayBladeTestScene : MonoBehaviour
{
	[SerializeField]
	private Transform _playbladeScaffold;

	[SerializeField]
	private PlayBladeV3 _playBladePrefab;

	[SerializeField]
	private GameObject _wwiseGlobalListener;

	private PlayBladeV3 _playBlade;

	private IBladeModel fakeData = new FakeBladeData
	{
		Events = new List<BladeEventInfo>
		{
			new BladeEventInfo
			{
				EventName = "Fake Event",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
				LocShortTitle = "Faker Than Fake",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Event",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_KLR.png",
				LocShortTitle = "XLR Scam",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Sealed_KLR.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Shake",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_AKH.png",
				LocShortTitle = "Faker Shake",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Standard_Shakeup.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Event",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_KLD.png",
				LocShortTitle = "Treasure Faker",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Treasure_Constructed.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Event",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
				LocShortTitle = "Faker Than Fake",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Event",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_KLR.png",
				LocShortTitle = "XLR Scam",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Sealed_KLR.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Shake",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_AKH.png",
				LocShortTitle = "Faker Shake",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Standard_Shakeup.jpg"
			},
			new BladeEventInfo
			{
				EventName = "Fake Event",
				LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_KLD.png",
				LocShortTitle = "Treasure Faker",
				BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Treasure_Constructed.jpg"
			}
		},
		Decks = new List<DeckViewInfo>
		{
			new DeckViewInfo
			{
				deckImageAssetPath = null,
				crop = ArtCrop.DEFAULT,
				deckName = "BlueFake",
				deckId = Guid.Empty,
				manaColors = new List<ManaColor> { ManaColor.Blue },
				sleeveData = null
			},
			new DeckViewInfo
			{
				deckImageAssetPath = null,
				crop = ArtCrop.DEFAULT,
				deckName = "RedFake",
				deckId = Guid.Empty,
				manaColors = new List<ManaColor> { ManaColor.Red },
				sleeveData = null
			},
			new DeckViewInfo
			{
				deckImageAssetPath = null,
				crop = ArtCrop.DEFAULT,
				deckName = "BlackFake",
				deckId = Guid.Empty,
				manaColors = new List<ManaColor> { ManaColor.Black },
				sleeveData = null
			}
		},
		Ranks = new List<RankViewInfo>(),
		Queues = new Dictionary<PlayBladeQueueType, List<BladeQueueInfo>>
		{
			{
				PlayBladeQueueType.Ranked,
				new List<BladeQueueInfo>
				{
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Standard B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Standard B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						EventInfo_BO3 = new BladeEventInfo
						{
							EventName = "Fake Standard B03",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Standard B03",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Standard"
					},
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Alchemy B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Alchemy B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						EventInfo_BO3 = new BladeEventInfo
						{
							EventName = "Fake Alchemy B03",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Alchemy B03",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Alchemy"
					},
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Historic B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Historic B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						EventInfo_BO3 = new BladeEventInfo
						{
							EventName = "Fake Historic B03",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Historic B03",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Historic"
					}
				}
			},
			{
				PlayBladeQueueType.Unranked,
				new List<BladeQueueInfo>
				{
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Standard B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Standard B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						EventInfo_BO3 = new BladeEventInfo
						{
							EventName = "Fake Standard B03",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Standard B03",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Standard"
					},
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Alchemy B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Alchemy B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						EventInfo_BO3 = new BladeEventInfo
						{
							EventName = "Fake Alchemy B03",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Alchemy B03",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Alchemy"
					},
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Historic B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Historic B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						EventInfo_BO3 = new BladeEventInfo
						{
							EventName = "Fake Historic B03",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Historic B03",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Historic"
					},
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Bot Match",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Bot Match",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "BotMatch"
					}
				}
			},
			{
				PlayBladeQueueType.Brawl,
				new List<BladeQueueInfo>
				{
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Standard Brawl B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Standard Brawl B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Standard"
					},
					new BladeQueueInfo
					{
						EventInfo_BO1 = new BladeEventInfo
						{
							EventName = "Fake Historic B01",
							LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
							LocShortTitle = "Faker Than Fake Historic Brawl B01",
							BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
						},
						LocTitle = "Historic"
					}
				}
			}
		},
		RecentlyPlayed = new List<RecentlyPlayedInfo>
		{
			new RecentlyPlayedInfo
			{
				EventInfo = new BladeEventInfo
				{
					EventName = "Fake Event",
					LogoImagePath = "Assets/Core/Art/Shared/Images/SetLogos/SetLogo_XLN.png",
					LocShortTitle = "Faker Than Fake",
					BladeImagePath = "Assets/Core/Art/Meta/Backgrounds/Events/BackgroundFull_Event_Metagame_Challenge.jpg"
				},
				SelectedDeckInfo = new DeckViewInfo
				{
					deckImageAssetPath = null,
					crop = ArtCrop.DEFAULT,
					deckName = "BlueFake",
					deckId = Guid.Empty,
					manaColors = new List<ManaColor> { ManaColor.Blue },
					sleeveData = null
				}
			}
		},
		EventFilters = new List<BladeEventFilter>
		{
			new BladeEventFilter
			{
				LocTitle = "PlayBlade/Filters/Default/All",
				FilterCriteria = "",
				FilterType = EventFilterType.All
			},
			new BladeEventFilter
			{
				LocTitle = "PlayBlade/Filters/Default/InProgress",
				FilterCriteria = "",
				FilterType = EventFilterType.InProgress
			},
			new BladeEventFilter
			{
				LocTitle = "PlayBlade/Filters/Default/New",
				FilterCriteria = "",
				FilterType = EventFilterType.New
			}
		}
	};

	private BladeSelectionInfo fakeSelectionInfo = new BladeSelectionInfo();

	private void Start()
	{
		_playBlade = UnityEngine.Object.Instantiate(_playBladePrefab, _playbladeScaffold);
		UnityEngine.Object.Instantiate(_wwiseGlobalListener);
		_playBlade.Initialize(new NullCardBuilder<Meta_CDC>(), Pantry.Get<IPlayBladeSelectionProvider>(), JoinMatchCallback, EditDeckCallback, GoToEventPageCallback, PlayBladeQueueSelectedCallback, PlayBladeFilterSelectedCallback, null, default(AssetLookupSystem));
		_playBlade.SetData(fakeData);
		_playBlade.Show();
	}

	private void JoinMatchCallback(string eventName, Guid deckId)
	{
		Debug.Log($"JoinMatch => event: {eventName} deck: {deckId}");
	}

	private void EditDeckCallback(Guid deckId, string eventId, string eventFormat, bool isInvalidForFormat)
	{
		Console.WriteLine($"EDITDECK => deck: {deckId} event:{eventId} event format:{eventFormat}");
	}

	private void GoToEventPageCallback(string eventName)
	{
		Debug.Log("GOTOEVENT => " + eventName);
	}

	private void PlayBladeQueueSelectedCallback(BladeEventInfo selectedBladeEventInfo)
	{
		Debug.Log("BladeQueueSelected => " + JsonConvert.SerializeObject(selectedBladeEventInfo));
	}

	private void PlayBladeFilterSelectedCallback(BladeEventFilter selectedFilter)
	{
		Debug.Log("BladeFilterSelected => " + JsonConvert.SerializeObject(selectedFilter));
	}

	private void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(_playBlade);
	}
}
