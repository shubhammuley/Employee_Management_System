using BaseLibrary.DTOs;
using ClientLibrary.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
    public class CustomHttpHandler(GetHttpClient getHttpClient, LocalStorageService localStorageService, IUserAccountService userAccountService) : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool loginUrl = request.RequestUri!.AbsoluteUri.Contains("login");
            bool registerUrl = request.RequestUri!.AbsoluteUri.Contains("regitser");
            bool refreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains("refresh-token");

            if (loginUrl || registerUrl || refreshTokenUrl)
            return await base.SendAsync(request, cancellationToken);

            var result = await base.SendAsync(request, cancellationToken);

            if(result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Get Token from localStorage
                var stringToken = await localStorageService.GetToken();
                if (stringToken == null) return result;

                //check if header contains token
                string token = string.Empty;
                try { token = request.Headers.Authorization!.Parameter; }
                catch { }

                var deserializedToken = Seriallizations.DeserializeJsonString<UserSession>(stringToken);
                if(deserializedToken is null) return result;    

                if(string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer",deserializedToken.Token);
                    return await base.SendAsync(request, cancellationToken);
                }

                //call for refresh token
                var newJwtToken = await getRefreshToken(deserializedToken!.RefreshToken!);
                if(string.IsNullOrEmpty(newJwtToken)) return result;

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newJwtToken);
                return await base.SendAsync(request, cancellationToken);

            }
            return result;
        }

        private async Task<string> getRefreshToken(string refreshToken)
        {
            var result = await userAccountService.RefreshTokenAsync(new RefreshToken()
            {
                Token = refreshToken,
            });
            string serializedToken = Seriallizations.SerializeObj(new UserSession() { Token = result.Token, RefreshToken = result.RefreshToken });
            await localStorageService.SetToken(serializedToken); 
            return result.Token;


        }
    }
}
