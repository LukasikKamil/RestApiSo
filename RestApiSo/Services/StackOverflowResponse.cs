using RestApiSo.Models;
using System.Text.Json.Serialization;

namespace RestApiSo.Services;

public class StackOverflowResponse
{
    [JsonPropertyName("items")]
    public List<Tag>? Items { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
}