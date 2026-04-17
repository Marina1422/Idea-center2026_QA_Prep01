
using System.Text.Json.Serialization;

namespace ExamPrepIdeaCenter.Models
{
    internal class IdeaDTO
    {
        [JsonPropertyName("title")] //label
        public string? Title { get; set; }


        [JsonPropertyName("description")] //label
        public string? Description { get; set; }


        [JsonPropertyName("url")] //label
        public string? URL { get; set; }
    }
}
