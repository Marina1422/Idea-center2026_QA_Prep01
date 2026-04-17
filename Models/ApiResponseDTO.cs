
using System.Text.Json.Serialization;

namespace ExamPrepIdeaCenter.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")] //label
        public string? Msg { get; set; }


        [JsonPropertyName("id")] //label
        public string? Id { get; set; }
    }
}
