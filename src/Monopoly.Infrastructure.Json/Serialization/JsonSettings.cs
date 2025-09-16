using System.Text.Json;

namespace Monopoly.Infrastructure.Json.Serialization;

public static class JsonSettings
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
}
