using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.IO.Compression;

namespace src
{
    public class Bcl
    {
        /// <summary>
        /// Returns JSON for component/measure from the BCL library.
        /// </summary>
        /// <param name="uuid">UUID in form:  xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx, lowercase letters.</param>
        /// <returns>Server response in JSON</returns>
        /// <exception cref="HttpRequestException"></exception>
        public string GetByUUID(string uuid)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Host = "bcl.nrel.gov";
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            // The server expects the UUID to use lowercase letters.
            HttpResponseMessage response = client.GetAsync("https://bcl.nrel.gov/api/search/?fq=uuid:" + uuid.ToLower(), HttpCompletionOption.ResponseContentRead).Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string message = $"Did not receive an OK back from bcl.nrel.gov HTTP request. Response items:\nStatus Code: {response.StatusCode}\nReason: {response.ReasonPhrase}\nHeaders: {response.Headers}";
                throw new HttpRequestException(message);
            }

            Stream responseStream;
            if (response.Content.Headers.TryGetValues("Content-Encoding", out IEnumerable<string> values) && values.Contains("gzip"))
            {
                // Uncompress gzip response
                responseStream = new GZipStream(response.Content.ReadAsStream(), CompressionMode.Decompress);
            }
            else
            {
                responseStream = response.Content.ReadAsStream();
            }

            StreamReader reader = new(responseStream, Encoding.UTF8);

            string allContents = reader.ReadToEnd();
            return allContents;
        }
    }
}
