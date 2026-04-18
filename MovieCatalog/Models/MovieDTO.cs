using System.Text.Json.Serialization;

namespace MovieCatalog.Models
{
    internal class MovieDTO
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("posterUrl")]
        public string? PosterUrl { get; set; }

        [JsonPropertyName("trailerLink")]
        public string? TrailerLink { get; set; }

        [JsonPropertyName("isWatched")]
        public bool? IsWatched { get; set; }
    }
}