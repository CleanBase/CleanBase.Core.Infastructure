using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Settings;
using CleanBase.Core.ViewModels.Response.Generic;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Services.KeyVault
{
	public class KeyVaultService : IKeyVaultService
	{
		private readonly AppSettings _appSettings;
		private readonly RestClient _client;

		public KeyVaultService(AppSettings appSettings)
		{
			_appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
			_client = new RestClient();
		}

		public async Task<AppSettings?> GetConfig()
		{
			if (string.IsNullOrEmpty(_appSettings.KeyVaultApiUrl))
				throw new InvalidOperationException("KeyVaultApiUrl must be provided in AppSettings.");

			var requestUrl = $"{_appSettings.KeyVaultApiUrl.TrimEnd('/')}/{_appSettings.AppId}";
			var request = new RestRequest(requestUrl);

			try
			{
				var response = await _client.ExecuteAsync(request);

				if (!response.IsSuccessful)
					throw new HttpRequestException($"Error fetching KeyVault config: {response.ErrorMessage}");

				var actionResponse = JsonConvert.DeserializeObject<ActionResponse<KeyVaultResponse>>(response.Content);

				if (actionResponse?.Result?.config == null)
					throw new JsonSerializationException("Failed to deserialize the KeyVault config.");

				return actionResponse.Result.config;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Failed to retrieve configuration from KeyVault.", ex);
			}
		}
	}
}
