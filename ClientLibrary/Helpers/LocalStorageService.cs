
using Blazored.LocalStorage;

namespace ClientLibrary.Helpers
{
    public class LocalStorageService(ILocalStorageService localStorageService)
    {
        private const string storageKey = "authentication-token";

        public async Task<string> GetToken() => await localStorageService.GetItemAsStringAsync(storageKey);

        public async Task SetToken(string item) => await localStorageService.SetItemAsStringAsync(storageKey, item);

        public async Task RemoveToken() => await localStorageService.RemoveItemAsync(storageKey);


    }
}
