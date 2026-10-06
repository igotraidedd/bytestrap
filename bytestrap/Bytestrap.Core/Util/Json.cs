using System.Text.Json;

namespace Bytestrap.Core.Util;

/// <summary>Shared JSON helpers with cached options (options are expensive to build).</summary>
public static class Json
{
    public static class Options
    {
        private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions _compact = new() { WriteIndented = false };

        public static string WriteIndented<T>(T value) => JsonSerializer.Serialize(value, _indented);

        public static string Write<T>(T value) => JsonSerializer.Serialize(value, _compact);

        public static T? Read<T>(string json) => JsonSerializer.Deserialize<T>(json);
    }
}
