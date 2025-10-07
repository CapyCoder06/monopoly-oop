using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monopoly.Infrastructure.Json.Serialization;

public static class JsonSettings
{
    public static JsonSerializerOptions Default => new()
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}
