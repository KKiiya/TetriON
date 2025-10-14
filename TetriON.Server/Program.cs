using Netly;

namespace TetriON.Server;

public static class Program {

    private static UDP.Server? _server;

    [STAThread]
    public static void Main(string[] args) {
        _server = new UDP.Server();
        _server.To.Open(new Host("127.0.0.1", 11000));


    }
}
