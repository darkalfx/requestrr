using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Requestrr.WebApi.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task ThrowIfNotSuccessfulAsync(this HttpResponseMessage responseMessage, string message, Func<dynamic, string> getErrorFunc)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                var textResponse = await responseMessage.Content.ReadAsStringAsync();
                dynamic jsonObject = null;

                try
                {
                    jsonObject = JObject.Parse(textResponse);
                }
                catch
                {
                    // Ignore
                }

                if (jsonObject != null)
                {
                    throw new Exception($"{message}: {responseMessage.ReasonPhrase} - {getErrorFunc(jsonObject) ?? textResponse}");
                }

                throw new Exception($"{message}: {responseMessage.ReasonPhrase} - {textResponse}");
            }

            return;
        }
    }
}