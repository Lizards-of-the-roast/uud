using Core.Meta.MainNavigation.Store;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Cosmetics;

public class PetUtils
{
	public const string PetNamePrefix = "MainNav/PetNames/";

	public static string KeyForPetDetails(PetEntry pet, IClientLocProvider locProvider)
	{
		return KeyForPetDetails(pet.Name, pet.Level, pet.Variant, locProvider);
	}

	public static string KeyForPetDetails(PetLevel pet, IClientLocProvider locProvider)
	{
		return KeyForPetDetails(pet.PetName, pet.Level, pet.VariantId, locProvider);
	}

	public static string KeyForPetDetails(string petName, int level, string variantId, IClientLocProvider locProvider)
	{
		string text = string.Format("{0}{1}_{2}", "MainNav/PetNames/", petName, level);
		string text2 = string.Format("{0}{1}_{2}", "MainNav/PetNames/", petName, variantId);
		string result = string.Format("{0}{1}", "MainNav/PetNames/", petName);
		if (locProvider.DoesContainTranslation(text2))
		{
			result = text2;
		}
		else if (locProvider.DoesContainTranslation(text))
		{
			result = text;
		}
		return result;
	}
}
