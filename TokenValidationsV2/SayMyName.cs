
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using Newtonsoft.Json;
using AnotherNameSpace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace TokenValidationsV2
{
    public static class SayMyName
    {
        [FunctionName("SayMyName")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route=null)]HttpRequestMessage req, TraceWriter log)
        {

            log.Info("C# HTTP trigger function processed a request.");

            // Authentication boilerplate code start
            ClaimsPrincipal principal;
            if ((principal = await AuthValidator.ValidateTokenAsync(req.Headers.Authorization)) == null)
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }
            // Authentication boilerplate code end

            // parse query parameter
            string queryStr = req.RequestUri.Query;
            if (!String.IsNullOrEmpty(queryStr))
            {
                
            }
            string name = req.RequestUri.Query;
            if (!string.IsNullOrEmpty(name) && name.StartsWith("?"))
            {
                name = name.Substring(1);
            }

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name = name ?? data?.name;

            return name == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(@"Please pass a name on the query string or in the request body")
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($"Hello {name}")
                };
        }
    }
}
