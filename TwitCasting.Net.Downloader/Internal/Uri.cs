using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace TwitCasting.Net.Downloader.Internal
{
    internal static class Uri
    {
        public static string BuildUri(string url, IDictionary<string, string> parameters = null)
        {
            var builder = new UriBuilder(url);

            if (parameters == null)
                return builder.ToString();

            var query = HttpUtility.ParseQueryString(builder.Query);

            BuildParameters(query, parameters);
            builder.Query = query.ToString() ?? throw new ArgumentNullException(nameof(query));

            return builder.ToString();
        }

        private static void BuildParameters(NameValueCollection collection, IDictionary<string, string> parameters)
        {
            foreach (var (key, parameter) in parameters)
                collection[key] = parameter;
        }
    }
}