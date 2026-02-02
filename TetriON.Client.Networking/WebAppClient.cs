using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TetriON.Client.Networking;

/// <summary>
/// HTTP client for communicating with the Go web app API
/// Handles accounts, lobbies, friends, etc.
/// </summary>
public class WebAppClient(string baseUrl) : IDisposable {
    private readonly HttpClient _httpClient = new();
    private string _baseUrl = baseUrl;

    public void Dispose() {
        _httpClient.Dispose();
        _baseUrl = string.Empty;
        GC.SuppressFinalize(this);
    }

    // TODO: Implement API methods for accounts, lobbies, friends
}
