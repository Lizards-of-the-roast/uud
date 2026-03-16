using GreClient.CardData;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga;

public class MetaCardBuilder : ICardBuilder<Meta_CDC>
{
	private readonly CardViewBuilder _cardBuilder;

	private readonly CardArtTextureLoader _textureLoader;

	private readonly IArtCropProvider _cropDatabase;

	public CardArtTextureLoader TextureLoader => _textureLoader;

	public IArtCropProvider CropDatabase => _cropDatabase;

	public MetaCardBuilder(CardViewBuilder cardViewBuilder)
	{
		_cardBuilder = cardViewBuilder;
		_textureLoader = _cardBuilder.CardMaterialBuilder.TextureLoader;
		_cropDatabase = _cardBuilder.CardMaterialBuilder.CropDatabase;
	}

	public Meta_CDC CreateCDC(ICardDataAdapter cardData, bool isVisible = false)
	{
		Meta_CDC meta_CDC = _cardBuilder.CreateMetaCdc(cardData);
		meta_CDC.gameObject.UpdateActive(isVisible);
		return meta_CDC;
	}

	public void DestroyCDC(Meta_CDC cdc)
	{
		_cardBuilder.DestroyCDC(cdc);
	}
}
