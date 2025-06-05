using System.Text.Json.Serialization;

namespace Web.Client.SnekLogic;

public class SeedDTO
{
    [JsonPropertyName("seed")]
    public int Seed { get; set; }
}