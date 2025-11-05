using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace maria.Dto
{
    public class CheckingItemReportRequest
    {
        [FromForm(Name = "items")]
        public string ItemsJson { get; set; }

        [FromForm(Name = "clientSignature")]
        public IFormFile? ClientSignature { get; set; }

        [FromForm(Name = "techSignature")]
        public IFormFile? TechSignature { get; set; }

        [FromForm(Name = "images")]
        public List<IFormFile>? Images { get; set; }

        [JsonIgnore]
        public List<CheckingItemDto> Items =>
            string.IsNullOrWhiteSpace(ItemsJson)
                ? new List<CheckingItemDto>()
                : System.Text.Json.JsonSerializer.Deserialize<List<CheckingItemDto>>(ItemsJson)!;
    }

    public class CheckingItemDto
    {
        public int CheckingItemId { get; set; }
        public string? Fault { get; set; }
        public string? CorrectiveAction { get; set; }
        public bool CorrectiveActionFlag { get; set; }
        public bool faultFlag { get; set; }
    }

    public class CheckItemReportFront
    {
        public int Id { get; set; }
        public int CheckingItemId { get; set; }
        public string? Fault { get; set; }
        public string? CorrectiveAction { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ClientSignaturePath { get; set; }
        public string? TechSignaturePath { get; set; }
    }

}
