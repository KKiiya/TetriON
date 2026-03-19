using System;

namespace TetriON.Server;

/// <summary>
/// Manages overall server state and configuration
/// </summary>
public class ServerState {
    public string ServerId { get; set; } = "";
    public string ServerName { get; set; } = "";
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public ServerStatus Status { get; set; }

    // TODO: Implement server state management
}

public enum ServerStatus {
    Starting,
    Running,
    Full,
    Stopping
}

public enum ServerRegion {
    // Europe
    EU_West,        // UK, France, Germany, Spain, Portugal
    EU_East,        // Poland, Romania, Czech Republic, Hungary

    // North America
    NA_East,        // New York / Virginia
    NA_West,        // California / Oregon
    NA_Central,     // Texas / Illinois

    // Latin America
    LA_North,       // Mexico, Colombia, Venezuela
    LA_South,       // Brazil, Argentina, Chile

    // Asia
    AS_East,        // Japan, South Korea
    AS_Southeast,   // Singapore, Indonesia, Philippines, Thailand
    AS_South,       // India, Sri Lanka

    // Middle East
    ME_Central,     // UAE, Saudi Arabia, Turkey

    // Africa
    AF_North,       // Egypt, Morocco
    AF_South,       // South Africa

    // Oceania
    OCE,            // Australia, New Zealand
}
