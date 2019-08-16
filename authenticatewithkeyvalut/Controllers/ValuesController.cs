using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Serilog;

namespace authenticatewithkeyvalut.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
           
                string Message = "Your application description page.";
            try
            {
                int retries = 0;
                bool retry = false;
                System.Diagnostics.Trace.TraceError("Logs coming");
                Log.Write(Serilog.Events.LogEventLevel.Information, " --> Get -- >Values Controller");
                /* The next four lines of code show you how to use AppAuthentication library to fetch secrets from your key vault */
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync("https://veebhssecrets.vault.azure.net/secrets/firstKey")
                        .ConfigureAwait(false);
                Message = secret.Value;
                Log.Write(Serilog.Events.LogEventLevel.Information, " --> Get -- >Value "+ Message);
                var secretBundle = await keyVaultClient.GetCertificateAsync("https://veebhssecrets.vault.azure.net/","testpfx");
                //X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(secretBundle.Cer));
                Message += Environment.NewLine + secretBundle.Id + Environment.NewLine + secretBundle.SecretIdentifier.BaseIdentifier+Environment.NewLine;
            }
            /* If you have throttling errors see this tutorial https://docs.microsoft.com/azure/key-vault/tutorial-net-create-vault-azure-web-app */
            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            /// 
             
            catch (Exception keyVaultException)
            {
                Message += "Error Occurred"+keyVaultException.Message;
            }
            return Ok(Message);
        }

        // This method implements exponential backoff if there are 429 errors from Azure Key Vault
        private static long getWaitTime(int retryCount)
        {
            long waitTime = ((long)Math.Pow(2, retryCount) * 100L);
            return waitTime;
        }

        // This method fetches a token from Azure Active Directory, which can then be provided to Azure Key Vault to authenticate
        public async Task<string> GetAccessTokenAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://vault.azure.net");
            return accessToken;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
