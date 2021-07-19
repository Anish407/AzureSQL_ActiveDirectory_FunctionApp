using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace sqlconnectapp
{
    public static class Function1
    {
        
        public static HttpClient sqlClient = new HttpClient();

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string accessToken = await GetAzureSqlAccessToken();
                string connectionString = @"Data Source=<servername>.database.windows.net;Initial Catalog=<dbname>";
                using var connection = new SqlConnection(connectionString);
                connection.AccessToken=accessToken;
                connection.Open();
                var cmd=connection.CreateCommand();
                cmd.CommandText = "Select GetDate()";
                cmd.CommandType = System.Data.CommandType.Text;
                var result = await cmd.ExecuteScalarAsync();
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static async Task<string> GetAzureSqlAccessToken()
        {
            var tokenRequestContext = new TokenRequestContext(_azureSqlScopes);
            var tokenResult = await _credential.GetTokenAsync(tokenRequestContext, default);
            return tokenResult.Token;
        }

        private static readonly string[] _azureSqlScopes = new[]
        {
            "https://database.windows.net//.default"
        };

        private static readonly TokenCredential _credential = new DefaultAzureCredential();

    }
}
