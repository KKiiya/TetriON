using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TetriON.Server.Networking;

/// <summary>
/// HTTP client for communicating with the Go web app
/// Registers server, updates status, validates sessions
/// </summary>
public class WebAppClient {
    private readonly HttpClient _httpClient;
    private string _baseUrl;

    public WebAppClient(string baseUrl) {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    // TODO: Implement web app communication methods
}
