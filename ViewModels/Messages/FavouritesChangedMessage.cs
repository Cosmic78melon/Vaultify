namespace Vaultify.ViewModels.Messages;

public record FavouritesChangedMessage(string SiteName, string password, bool isFavourite);