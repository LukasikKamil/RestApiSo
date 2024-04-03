using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestApiSo.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [JsonPropertyName("has_synonyms")]
        public bool HasSynonyms { get; set; }

        [JsonPropertyName("is_moderator_only")]
        public bool IsModeratorOnly { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("percentage")]
        public decimal Percentage { get; set; }

    }
}
