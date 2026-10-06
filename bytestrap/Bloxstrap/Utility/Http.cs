namespace Bloxstrap.Utility
{
    internal static class Http
    {
        /// <summary>
        /// Gets and deserializes a JSON API response to the specified object.
        /// Responses are disposed (frees the socket promptly) and deserialized
        /// straight from the network stream instead of buffering a giant string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="JsonException"></exception>
        public static async Task<T> GetJson<T>(Uri url)
        {
            using var response = await App.HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false))!;
        }

        public static async Task<T> SendJson<T>(HttpRequestMessage requestMessage)
        {
            using var response = await App.HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false))!;
        }

        public static async Task<T> AuthGetJson<T>(Uri url)
        {
            using var response = await App.Cookies.AuthGet(url).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false))!;
        }

        public static async Task<T> AuthSendJson<T>(HttpRequestMessage requestMessage)
        {
            HttpContent content = requestMessage.Content!;

            using var response = await App.Cookies.AuthPost(requestMessage.RequestUri, content).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync<T>(stream).ConfigureAwait(false))!;
        }
    }
}
