using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http;

namespace ALedgerBFFApi.Extension
{
    public static class HttpClientExtension
    {
        public static bool PassHeaders(this System.Net.Http.HttpClient httpClient, HttpRequest request)
        {
            if (request?.Headers == null) return false;

            var authHeader = request.Headers.Authorization.ToString()?.Replace("SigTx ", "");
            if (string.IsNullOrEmpty(authHeader)) return false;

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SigTx", authHeader);
            return true;
        }
    }
}
