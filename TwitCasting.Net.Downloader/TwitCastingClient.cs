using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Uri = TwitCasting.Net.Downloader.Internal.Uri;

namespace TwitCasting.Net.Downloader
{
    public class TwitCastingClient
    {
        protected static HttpClient Client => LazyHttpClient.Value;

        protected static Options Options { get; set; }

        public TwitCastingClient(Options options)
        {
            Options = options;
        }

        protected async Task<T> Get<T>(string url, IDictionary<string, string> parameters = null) where T : class
        {
            var response = await GetAsync(url, parameters).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(content);
        }

        protected HttpRequestMessage CreateRequestMessage(string requestUri, IDictionary<string, string> parameters = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri(Uri.BuildUri(requestUri, parameters)),
                Headers =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("text/html")
                    }
                }
            };

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(string url, IDictionary<string, string> parameters = null)
        {
            var response = await Client.SendAsync(CreateRequestMessage(url, parameters)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return response;
        }

        private async Task<HttpResponseMessage> GetAsync(string url, IDictionary<string, string> parameters = null)
            => await SendAsync(url, parameters).ConfigureAwait(false);

        private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            }, true);

            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");

            return httpClient;
        });
    }
}