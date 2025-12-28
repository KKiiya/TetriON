using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TetriON.Client.Networking {
    /// <summary>
    /// HTTP client for communicating with the Go web app API
    /// Handles accounts, lobbies, friends, etc.
    /// </summary>
    public class WebAppClient {
        private readonly HttpClient _httpClient;
        private string _baseUrl;

        public WebAppClient(string baseUrl) {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        // TODO: Implement API methods for accounts, lobbies, friends
    }
}
