using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace TetriON.Shared.Utilities;

/// <summary>
/// JSON serialization utilities for network messages
/// </summary>
public static class JsonSerializer {
    private static readonly JsonSerializerOptions _options = new() {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    // TODO: Implement serialization methods
}
