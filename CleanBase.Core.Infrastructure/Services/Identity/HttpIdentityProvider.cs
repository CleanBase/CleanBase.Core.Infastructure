using CleanBase.Core.Constants;
using CleanBase.Core.Domain;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Settings;
using System;
using System.Collections.Generic;
using RestSharp;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CleanBase.Core.ViewModels.Response;
using Newtonsoft.Json;
using CleanBase.Core.ViewModels.Response.Generic;

namespace CleanBase.Core.Infrastructure.Services.Identity
{
    public class HttpIdentityProvider : IdentityProvider
    {
        private readonly AppSettings _appSettings;
        private readonly ICacheManagerProvider _cacheManagerProvider;

        public HttpIdentityProvider(AppSettings appSettings, ICacheManagerProvider cacheManagerProvider)
        {
            _appSettings = appSettings;
            _cacheManagerProvider = cacheManagerProvider;
        }

        public override async Task UpdateIdentity(ClaimsPrincipal user, string token)
        {
            if (string.IsNullOrEmpty(_appSettings.Auth.UserProfileApiUrl))
            {
                throw new Exception("_appSettings.Auth.UserProfileApiUrl must not be null");
            }

            await base.UpdateIdentity(user, token);

            var userProfile = await _cacheManagerProvider.GetOrCreateAsync<UserProfileBasic>(
                async () => await FetchUserProfileAsync(token),
                CacheKeys.UserProfiles,
                Identity?.Sub
            );

            Identity.Permissions = userProfile?.Roles?
                .SelectMany(role => role.Permissions)
                .Distinct()
                .OrderBy(p => p)
                .ToList();
        }

        private async Task<UserProfileBasic?> FetchUserProfileAsync(string token)
        {
            var client = new RestClient();
            client.AddDefaultHeader("Authorization", $"Bearer {token}");

            var request = new RestRequest(_appSettings.Auth.UserProfileApiUrl);
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to fetch user profile: {response.ErrorMessage}");
            }

            var actionResponse = JsonConvert.DeserializeObject<ActionResponse<UserProfileBasic>>(response.Content);
            return actionResponse?.Result;
        }
    }
}
