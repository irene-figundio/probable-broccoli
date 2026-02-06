using System.Collections.Generic;

namespace WorkBotAI.Repositories.DTOs
{
    public class GlobalSearchResultDto
    {
        public List<SearchResultItemDto> Tenants { get; set; } = new();
        public List<SearchResultItemDto> Users { get; set; } = new();
        public int TotalResults => Tenants.Count + Users.Count;
    }

    public class SearchResultItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "tenant" or "user"
        public string? AdditionalInfo { get; set; }
    }
}
