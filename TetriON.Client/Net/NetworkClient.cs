using System;
using System.Threading.Tasks;
using Netly;

namespace TetriON.Client;

public class NetworkClient {

    private readonly UDP.Client _client;
    private const int MAX_RETRIES = 5;
    private int _retries;
    private byte _state = 0; // 0 = disconnected, 1 = connecting, 2 = connected, 3 = logged in

    public NetworkClient(string username, string password) {
        _state = 1; // Connecting
        _client = Connect("127.0.0.1", 11000).Result;
        LogIn(username, password);
    }

    public void LogIn(string username, string password) {
        // Implementation for logging in
        // after successful login:
        _state = 3; // Logged in
    }

    private async Task<UDP.Client> Connect(string host, int port) {
        if (_retries >= MAX_RETRIES) {
            _state = 0; // Disconnected (OFFLINE)
            throw new Exception("Max connection attempts exceeded.");
        }
        UDP.Client client = new();
        try {
            await client.To.Open(new Host(host, port));
            _state = 2; // Connected
        } catch (Exception ex) {
            Console.WriteLine($"Connection failed: {ex.Message}");
            _retries++;
            await Task.Delay(2000); // Wait before retrying
            return await Connect(host, port);
        }
        return client;
    }

    protected void SendMessage(string message) {
        if (_client == null) return;
        _client.To.Data(message);
    }

}
